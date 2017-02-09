using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Stackoverflow._18096050
{
    public sealed class StopComboBoxMemoryLeakBehaviour : Behavior<ComboBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            var combo = AssociatedObject;
            if (ReferenceEquals(combo, null))
            {
                return;
            }
            combo.DataContextChanged += combo_DataContextChanged;
        }

        protected override void OnDetaching()
        {
            var combo = AssociatedObject;
            combo.DataContextChanged -= combo_DataContextChanged;
            base.OnDetaching();
        }

        void combo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (ReferenceEquals(combo, null))
            {
                return;
            }
            if (ReferenceEquals(combo.Template, null))
            {
                combo.ApplyTemplate();
            }
            if (!ReferenceEquals(combo.Template, null))
            {
                var popup = combo.Template.FindName("PART_Popup", combo) as Popup;
                if (ReferenceEquals(popup, null))
                {
                    combo.ApplyTemplate();
                    popup = combo.Template.FindName("PART_Popup", combo) as Popup;
                }
                if (!ReferenceEquals(popup, null))
                {
                    SetPopupRootDataContextToNull(popup);
                }
            }
        }

        private static Action<Popup> _createNewPopupRootMethod;
        private static Action<Popup> CreateNewPopupRootMethod
        {
            get
            {
                if (ReferenceEquals(_createNewPopupRootMethod, null))
                {
                    var createNewPopupRootMethodInfo = typeof(Popup).GetMethod("CreateNewPopupRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    _createNewPopupRootMethod = (Action<Popup>)Delegate.CreateDelegate(typeof(Action<Popup>), createNewPopupRootMethodInfo);

                }
                return _createNewPopupRootMethod;
            }
        }

        private static void ForceCreationOfPopupRoot(Popup popup)
        {
            CreateNewPopupRootMethod(popup);
        }

        private static void SetPopupRootDataContextToNull(Popup popup)
        {
            var popupRootWrapperFieldInfo = typeof(Popup).GetField("_popupRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var popupRootWrapper = popupRootWrapperFieldInfo.GetValue(popup);
            if (popupRootWrapper != null)
            {
                var valueFieldInfo = popupRootWrapper.GetType().GetProperty("Value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var popupRoot = valueFieldInfo.GetValue(popupRootWrapper, new object[0]) as FrameworkElement;
                if (ReferenceEquals(popupRoot, null))
                {
                    ForceCreationOfPopupRoot(popup);
                    popupRootWrapper = popupRootWrapperFieldInfo.GetValue(popup);
                    popupRoot = valueFieldInfo.GetValue(popupRootWrapper, new object[0]) as FrameworkElement;
                }
                if (popupRoot != null)
                {
                    popupRoot.DataContext = null;
                }
            }
        }
    }
}
