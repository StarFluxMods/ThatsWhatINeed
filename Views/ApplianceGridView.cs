using System.Collections.Generic;
using Controllers;
using Kitchen;
using Kitchen.Modules;
using KitchenData;
using KitchenMods;
using MessagePack;
using ThatsWhatINeed.Components;
using ThatsWhatINeed.Menus;
using ThatsWhatINeed.Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ThatsWhatINeed.Views
{
    public class ApplianceGridView : ResponsiveObjectView<ApplianceGridView.ViewData, ApplianceGridView.ResponseData>, IInputConsumer
    {
        public class UpdateView : ResponsiveViewSystemBase<ViewData, ResponseData>, IModSystem
        {
            EntityQuery Views;
            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(typeof(CApplianceGridInfo), typeof(CLinkedView));
            }

            private List<int> available = new List<int>();
            private List<int> marked = new List<int>();
            
            protected override void OnUpdate()
            {
                using NativeArray<Entity> entities = Views.ToEntityArray(Allocator.Temp);
                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using NativeArray<CApplianceGridInfo> infos = Views.ToComponentDataArray<CApplianceGridInfo>(Allocator.Temp);

                foreach (Entity entity in entities)
                {
                    if (!Require(entity, out CLinkedView cLinkedView)) continue;
                    if (!Require(entity, out CApplianceGridInfo cPetEditorInfo)) continue;
                    if (TryGetSingletonEntity<SAvailableBlueprint>(out Entity entity1) && TryGetSingletonEntity<SMarkedBlueprint>(out Entity entity2))
                    {
                        DynamicBuffer<SAvailableBlueprint> sAvailableBlueprint = GetBuffer<SAvailableBlueprint>(entity1);
                        DynamicBuffer<SMarkedBlueprint> sMarkedBlueprint = GetBuffer<SMarkedBlueprint>(entity2);
                        
                        available.Clear();
                        marked.Clear();
                        
                        foreach (SAvailableBlueprint blueprint in sAvailableBlueprint)
                        {
                            available.Add(blueprint);
                        }
                        
                        foreach (SMarkedBlueprint blueprint in sMarkedBlueprint)
                        {
                            marked.Add(blueprint);
                        }
                        
                        SendUpdate(cLinkedView.Identifier, new ViewData
                        {
                            Player = cPetEditorInfo.Player.PlayerID,
                            AvailableOptions = available,
                            MarkedOptions = marked
                        });
                    
                        ResponseData result = default(ResponseData);

                        if (ApplyUpdates(cLinkedView, (data) => { result = data; }, only_final_update: true))
                        {
                            cPetEditorInfo.IsComplete = result.IsComplete;
                            Set(entity, cPetEditorInfo);
                            ToggleMarkedAppliance.instance.ToggleApplianceID(result.Option);
                        }
                    }
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : IViewData, IViewResponseData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)]
            public int Player;
            [Key(1)]
            public List<int> AvailableOptions;
            [Key(2)]
            public List<int> MarkedOptions;

            public bool IsChangedFrom(ViewData check)
            {
                return Player != check.Player || !AvailableOptions.Equals(check.AvailableOptions) || !MarkedOptions.Equals(check.MarkedOptions);
            }
        }

        [MessagePackObject(false)]
        public struct ResponseData : IResponseData, IViewResponseData
        {
            [Key(0)]
            public bool IsComplete;
            [Key(1)]
            public int Option;
            [Key(2)]
            public int PlayerID;
        }

        private struct MenuStackElement
        {
            public ApplianceGridConfig Config;

            public int Index;

            public int Offset;
        }

        public ApplianceGridConfig rootMenuConfig;
        public Transform container;
        private ApplianceGridMenu _gridMenu;
        private int _playerID;
        private InputLock.Lock _lock;
        private bool _isComplete;
        private readonly Stack<MenuStackElement> _menuStack = new Stack<MenuStackElement>();
        private int _option;

        private void CloseMenu(bool force, List<int> AvailableOptions, List<int> MarkedOptions)
        {
            if (_menuStack.Count > 1 && !force)
            {
                int index = _menuStack.Pop().Index;
                MenuStackElement menuStackElement = _menuStack.Pop();
                SetNewMenu(menuStackElement.Config, index, menuStackElement.Index, menuStackElement.Offset, AvailableOptions, MarkedOptions);
            }
            else
            {
                Remove();
            }
        }

        private void SetNewMenu(ApplianceGridConfig menu, int newIndex, int previousIndex, int offset, List<int> AvailableOptions, List<int> MarkedOptions, bool addToStack = true)
        {
            _gridMenu?.Destroy();
            _gridMenu = menu.Instantiate(delegate (int result)
            {
                if (GameData.Main.TryGet(result, out Appliance appliance))
                {
                    _option = result;
                    CloseMenu(true, AvailableOptions, MarkedOptions);
                }
                else
                {
                    if (result == 0)
                    {
                        CloseMenu(false, AvailableOptions, MarkedOptions);
                    }else if (result == 3)
                    {
                        SetNewMenu(menu, 0, 0, offset + 16, AvailableOptions, MarkedOptions);
                    }
                }
            }, container, _playerID, _menuStack.Count > 0, AvailableOptions, MarkedOptions, offset);
            
            _gridMenu.OnRequestMenu += delegate (GridMenuConfig c)
            {
                if (c is ApplianceGridConfig cApp)
                    SetNewMenu(cApp, 0, _gridMenu?.SelectedIndex() ?? 0, 0, AvailableOptions, MarkedOptions);
            };
            _gridMenu.OnGoBack += delegate { CloseMenu(false, AvailableOptions, MarkedOptions); };
            _gridMenu.SelectByIndex(newIndex);
            
            if (addToStack)
            {
                _menuStack.Push(new MenuStackElement
                {
                    Config = menu,
                    Index = previousIndex,
                    Offset = offset
                });
            }
        }

        protected override void UpdateData(ViewData data)
        {
            if (InputSourceIdentifier.DefaultInputSource == null) return;
            
            if (!Players.Main.Get(data.Player).IsLocalUser)
            {
                gameObject.SetActive(value: false);
                return;
            }
            gameObject.SetActive(value: true);
            _option = 0;
            InitialiseForPlayer(data.Player, data.AvailableOptions, data.MarkedOptions);
        }

        private void InitialiseForPlayer(int player, List<int> AvailableOptions, List<int> MarkedOptions)
        {
            LocalInputSourceConsumers.Register(this);
            if (_lock.Type != 0)
                InputSourceIdentifier.DefaultInputSource.ReleaseLock(_playerID, _lock);
            _playerID = player;
            SetNewMenu(rootMenuConfig, 0, 0, 0, AvailableOptions, MarkedOptions);
            _lock = InputSourceIdentifier.DefaultInputSource.SetInputLock(_playerID, PlayerLockState.NonPause);
        }

        public override void Remove()
        {
            _isComplete = true;
            InputSourceIdentifier.DefaultInputSource.ReleaseLock(_playerID, _lock);
            base.Remove();
        }

        private void OnDestroy()
        {
            LocalInputSourceConsumers.Remove(this);
        }

        public InputConsumerState TakeInput(int playerID, InputState state)
        {
            if (_playerID == 0 || playerID != _playerID) return InputConsumerState.NotConsumed;
            if (state.MenuTrigger == ButtonState.Pressed)
            {
                _isComplete = true;
                InputSourceIdentifier.DefaultInputSource.ReleaseLock(_playerID, _lock);
                return InputConsumerState.Terminated;
            }
            if (_gridMenu != null && !_gridMenu.HandleInteraction(state) && state.MenuCancel == ButtonState.Pressed)
            {
                CloseMenu(true, default, default);
            }
            return !_isComplete ? InputConsumerState.Consumed : InputConsumerState.Terminated;
        }
        
        public override bool HasStateUpdate(out IResponseData state)
        {
            state = null;
            if (_isComplete)
            {
                state = new ResponseData
                {
                    IsComplete = _isComplete,
                    Option = _option,
                    PlayerID = _playerID
                };
            }
            return _isComplete;
        }
    }
}
