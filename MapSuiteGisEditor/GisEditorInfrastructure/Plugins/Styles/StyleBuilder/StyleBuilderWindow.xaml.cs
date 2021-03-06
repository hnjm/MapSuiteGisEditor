/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ThinkGeo.MapSuite.Styles;

namespace ThinkGeo.MapSuite.GisEditor
{
    /// <summary>
    /// Interaction logic for StyleConfigurationWindow.xaml
    /// </summary>
    public partial class StyleBuilderWindow : Window
    {
        private int rootNodeMouseDownCount;
        private bool isSubStyleReadonly;
        private StyleLayerListItem draggingItemParent;
        private DispatcherTimer rootNodeClickTimer;
        private StyleBuilderViewModel styleBuilderViewModel;
        private StyleBuilderResult styleBuilderResult;
        private StyleBuilderArguments styleBuilderArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleBuilderWindow" /> class.
        /// </summary>
        public StyleBuilderWindow()
            : this(new StyleBuilderArguments())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleBuilderWindow" /> class.
        /// </summary>
        /// <param name="styleBuilderArguments">The style builder arguments.</param>
        public StyleBuilderWindow(StyleBuilderArguments styleBuilderArguments)
        {
            ApplyWindowStyle(this);

            this.styleBuilderArguments = styleBuilderArguments;
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(StyleBuilderWindow_Loaded);
        }

        public bool IsSubStyleReadonly
        {
            get { return isSubStyleReadonly; }
            set { isSubStyleReadonly = value; }
        }

        /// <summary>
        /// Gets or sets the style builder arguments.
        /// </summary>
        /// <value>
        /// The style builder arguments.
        /// </value>
        public StyleBuilderArguments StyleBuilderArguments
        {
            get { return styleBuilderArguments; }
            set { styleBuilderArguments = value; }
        }

        /// <summary>
        /// Gets the style builder result.
        /// </summary>
        /// <value>
        /// The style builder result.
        /// </value>
        public StyleBuilderResult StyleBuilderResult
        {
            get { return styleBuilderResult; }
        }

        /// <summary>
        /// Gets the result style.
        /// </summary>
        /// <value>
        /// The result style.
        /// </value>
        private CompositeStyle ResultStyle
        {
            get { return styleBuilderViewModel.ResultStyle; }
        }

        /// <summary>
        /// starts from 1.
        /// </summary>
        private int FromZoomLevelIndex
        {
            get { return styleBuilderViewModel.FromZoomLevelIndex; }
            set { styleBuilderViewModel.FromZoomLevelIndex = value; }
        }

        /// <summary>
        /// starts from 1.
        /// </summary>
        private int ToZoomLevelIndex
        {
            get { return styleBuilderViewModel.ToZoomLevelIndex; }
            set { styleBuilderViewModel.ToZoomLevelIndex = value; }
        }

        /// <summary>
        /// Handles the Loaded event of the StyleBuilderWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void StyleBuilderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var styleToEdit = styleBuilderArguments.StyleToEdit;
            if (styleToEdit == null) styleToEdit = new CompositeStyle();

            styleBuilderResult = new StyleBuilderResult();
            styleBuilderViewModel = new StyleBuilderViewModel(styleToEdit, styleBuilderArguments);
            DataContext = styleBuilderViewModel;

            FromZoomLevelIndex = styleBuilderArguments.FromZoomLevelIndex;
            ToZoomLevelIndex = styleBuilderArguments.ToZoomLevelIndex;

            if (styleBuilderViewModel.RootStyleItem != null && styleBuilderViewModel.RootStyleItem.StyleItemViewModels.Count == 0)
            {
                if (Validate())
                {
                    styleBuilderViewModel.IsRootNodeSelected = true;
                    RootNodePanel.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230));
                }
            }
        }

        /// <summary>
        /// Handles the IsVisibleChanged event of the RenameTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void RenameTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.IsVisible)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Handles the Click event of the CancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            styleBuilderResult.Canceled = true;
        }

        /// <summary>
        /// Handles the Click event of the ConfirmButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                if (!e.Handled)
                {
                    styleBuilderResult = new StyleBuilderResult(ResultStyle, false, FromZoomLevelIndex, ToZoomLevelIndex);
                    DialogResult = true;
                }
            }
            else e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the ApplyButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                if (!IsValid(ConfigureLayout))
                {
                    System.Windows.Forms.MessageBox.Show(GisEditor.LanguageManager.GetStringResource("StyleBuilderWindowStyleInvalidText"));
                }
                else
                {
                    //OnApplied(e);
                    styleBuilderResult = new StyleBuilderResult();
                    styleBuilderResult.Canceled = false;
                    styleBuilderResult.FromZoomLevelIndex = FromZoomLevelIndex;
                    styleBuilderResult.ToZoomLevelIndex = ToZoomLevelIndex;
                    styleBuilderResult.CompositeStyle = ResultStyle;
                    styleBuilderViewModel.ApplyStyles(styleBuilderResult);
                }
            }
            else e.Handled = true;
        }

        /// <summary>
        /// Determines whether the specified obj is valid.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        ///   <c>true</c> if the specified obj is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid(DependencyObject obj)
        {
            if (Validation.GetHasError(obj))
            {
                return false;
            }
            else
            {
                var childrenCount = VisualTreeHelper.GetChildrenCount(obj);
                bool valid = true;
                for (int i = 0; i < childrenCount; i++)
                {
                    if (!IsValid(VisualTreeHelper.GetChild(obj, i)))
                    {
                        valid = false;
                        break;
                    }
                }

                return valid;
            }
        }

        /// <summary>
        /// Trees the view item mouse right button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void TreeViewItemMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            styleBuilderViewModel.IsRootNodeSelected = false;
            var treeViewItem = (TreeViewItem)sender;
            treeViewItem.IsSelected = true;
            e.Handled = true;
        }

        /// <summary>
        /// Trees the view item mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void TreeViewItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            styleBuilderViewModel.IsRootNodeSelected = false;
        }

        /// <summary>
        /// Trees the view_ selected item changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        [Obfuscation]
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            styleBuilderViewModel.SelectedStyleItem = e.NewValue as StyleItemViewModel;
            RootNodePanel.Background = null;
        }

        /// <summary>
        /// Handles the MouseDown event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Validate())
            {
                styleBuilderViewModel.IsRootNodeSelected = true;
                RootNodePanel.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230));

                InitializeDBClickTimer();
                if (e.ChangedButton == MouseButton.Left)
                {
                    rootNodeMouseDownCount++;
                }

                if (!rootNodeClickTimer.IsEnabled)
                {
                    rootNodeClickTimer.Start();
                }
            }
            else e.Handled = true;
        }

        /// <summary>
        /// Initializes the DB click timer.
        /// </summary>
        private void InitializeDBClickTimer()
        {
            if (rootNodeClickTimer == null)
            {
                rootNodeClickTimer = new DispatcherTimer();
                rootNodeClickTimer.Interval = TimeSpan.FromMilliseconds(500);
                rootNodeClickTimer.Tick += (s, args) =>
                {
                    if (rootNodeMouseDownCount > 1)
                    {
                        StyleItemViewModel renamingItemViewModel = null;
                        if (styleBuilderViewModel.IsRootNodeSelected)
                        {
                            renamingItemViewModel = styleBuilderViewModel.RootStyleItem;
                        }
                        else if (styleBuilderViewModel.SelectedStyleItem != null)
                        {
                            renamingItemViewModel = styleBuilderViewModel.SelectedStyleItem;
                        }

                        if (renamingItemViewModel != null)
                        {
                            renamingItemViewModel.IsRenaming = true;
                        }
                    }
                    rootNodeMouseDownCount = 0;
                    rootNodeClickTimer.Stop();
                };
            }
        }

        /// <summary>
        /// Applies the window style.
        /// </summary>
        /// <param name="window">The window.</param>
        [Conditional("GISEditorUnitTest")]
        private static void ApplyWindowStyle(Window window)
        {
            ResourceDictionary resourceDic = new ResourceDictionary();
            resourceDic.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/MapSuiteGisEditor;component/Resources/General.xaml", UriKind.RelativeOrAbsolute) });
            window.Resources = resourceDic;
        }

        [Obfuscation]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string uri = GisEditor.LanguageManager.GetStringResource("StyleBuilderHelp");
            Process.Start(uri);
        }

        [Obfuscation]
        private void Ribbon_Loaded(object sender, RoutedEventArgs e)
        {
            var child = VisualTreeHelper.GetChild(Ribbon, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        [Obfuscation]
        private void RenameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitRenaming(sender);
        }

        [Obfuscation]
        private void RenameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter || e.Key == Key.Tab)
            {
                CommitRenaming(sender);
            }
        }

        /// <summary>
        /// Commits the renaming.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private static void CommitRenaming(object sender)
        {
            FrameworkElement textBox = sender as FrameworkElement;
            if (textBox != null && textBox.DataContext is StyleItemViewModel)
            {
                var styleItemViewModel = (StyleItemViewModel)(textBox.DataContext);
                styleItemViewModel.CommitRenaming();
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            styleBuilderViewModel.RootStyleItem.CommitRenaming();
            if (styleBuilderViewModel.SelectedStyleItem != null)
            {
                styleBuilderViewModel.SelectedStyleItem.CommitRenaming();
            }
        }

        /// <summary>
        /// Trees the view item drop.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        [Obfuscation]
        private void TreeViewItemDrop(object sender, DragEventArgs e)
        {
            Grid item = sender as Grid;
            if (item != null)
            {
                var newStyleItemParent = ((StyleItemViewModel)e.Data.GetData(typeof(StyleItemViewModel))).StyleItem.Parent as StyleLayerListItem;
                if (newStyleItemParent != null)
                {
                    if (draggingItemParent != null)
                    {
                        draggingItemParent.UpdateConcreteObject();
                        var parentUI = draggingItemParent.GetUI(styleBuilderViewModel.StyleArguments);
                        if (parentUI != null) draggingItemParent.UpdateUI(parentUI);
                    }

                    if (newStyleItemParent != draggingItemParent)
                    {
                        newStyleItemParent.UpdateConcreteObject();
                        var parentUI = newStyleItemParent.GetUI(styleBuilderViewModel.StyleArguments);
                        if (parentUI != null) newStyleItemParent.UpdateUI(parentUI);
                    }

                    styleBuilderViewModel.UpdatePreviewSource();
                }
            }
        }

        [Obfuscation]
        private void TreeViewItemDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            var dragingItemViewModel = (StyleItemViewModel)e.Data.GetData(typeof(StyleItemViewModel));
            var currentItemViewModel = ((StyleItemViewModel)(((Grid)sender)).DataContext);
            var dragingItem = dragingItemViewModel.StyleItem;
            var currentItem = currentItemViewModel.StyleItem;
            var dragingItemParent = dragingItem.Parent;
            var currentItemParent = currentItem.Parent;
            if (dragingItem != null && currentItem != null && dragingItem != currentItem && !IsParent(currentItem, dragingItem))
            {
                if (currentItem.CanContainStyleItem(dragingItem))
                {
                    dragingItemParent.Children.Remove(dragingItem);
                    currentItem.Children.Insert(0, dragingItem);
                    dragingItemViewModel = StyleItemViewModel.FindStyleItemViewModel(currentItemViewModel.ParentViewModel.StyleItemViewModels, dragingItem);
                }
                else if (dragingItemParent == currentItemParent)
                {
                    var dragingIndex = dragingItemParent.Children.IndexOf(dragingItem);
                    var currentIndex = dragingItemParent.Children.IndexOf(currentItem);
                    dragingItemParent.Children.Move(dragingIndex, currentIndex);
                }
                else
                {
                    if (currentItemParent is StyleLayerListItem && ((StyleLayerListItem)currentItemParent).CanContainStyleItem(dragingItem))
                    {
                        dragingItemParent.Children.Remove(dragingItem);
                        var currentIndex = currentItemParent.Children.IndexOf(currentItem);
                        currentItemParent.Children.Insert(currentIndex, dragingItem);
                        dragingItemViewModel = StyleItemViewModel.FindStyleItemViewModel(currentItemViewModel.ParentViewModel.StyleItemViewModels, dragingItem);
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }

                if (dragingItemViewModel != null) dragingItemViewModel.IsSelected = true;
            }
        }

        [Obfuscation]
        private void TreeViewItemMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            Grid item = sender as Grid;
            if (item != null && !item.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                var itemViewModel = (StyleItemViewModel)item.DataContext;
                if (!itemViewModel.IsRenaming)
                {
                    draggingItemParent = itemViewModel.StyleItem.Parent as StyleLayerListItem;
                    DragDrop.DoDragDrop(item, (itemViewModel), DragDropEffects.Move);
                    itemViewModel.IsSelected = true;
                }
            }
        }

        [Obfuscation]
        private void MainNodeDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            var dragingItemViewModel = (StyleItemViewModel)e.Data.GetData(typeof(StyleItemViewModel));
            var currentItemViewModel = ((StyleBuilderViewModel)(((Grid)sender)).DataContext);
            var dragingItem = dragingItemViewModel.StyleItem;
            var currentItem = currentItemViewModel.RootStyleItem.StyleItem;
            var dragingItemParent = dragingItem.Parent;
            var currentItemParent = currentItem.Parent;
            if (dragingItem != null && currentItem != null && dragingItem != currentItem && !IsParent(currentItem, dragingItem))
            {
                if (currentItem.CanContainStyleItem(dragingItem))
                {
                    dragingItemParent.Children.Remove(dragingItem);
                    currentItem.Children.Insert(0, dragingItem);
                    dragingItemViewModel = StyleItemViewModel.FindStyleItemViewModel(currentItemViewModel.RootStyleItem.StyleItemViewModels, dragingItem);
                }
            }

            if (dragingItemViewModel != null) dragingItemViewModel.IsSelected = true;
        }

        /// <summary>
        /// Determines whether the specified source item is parent.
        /// </summary>
        /// <param name="sourceItem">The source item.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>
        ///   <c>true</c> if the specified source item is parent; otherwise, <c>false</c>.
        /// </returns>
        private bool IsParent(StyleLayerListItem sourceItem, StyleLayerListItem parent)
        {
            bool isParent = false;
            while (sourceItem.Parent != null)
            {
                if (sourceItem.Parent == parent)
                {
                    isParent = true;
                    break;
                }
                else
                {
                    sourceItem = sourceItem.Parent as StyleLayerListItem;
                }
            }

            return isParent;
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>the result for validating</returns>
        private bool Validate()
        {
            if (styleBuilderViewModel.SelectedStyleItem != null)
            {
                var styleUserControl = styleBuilderViewModel.SelectedStyleItemUI as StyleUserControl;
                if (styleUserControl != null)
                {
                    return styleUserControl.Validate();
                }
            }
            return true;
        }

        [Obfuscation]
        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var styleItemViewModel = sender.GetDataContext<StyleItemViewModel>();
            if (styleItemViewModel != styleBuilderViewModel.SelectedStyleItem && !Validate()) e.Handled = true;
        }

        [Obfuscation]
        private void TreeViewItem_Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((Grid)sender).DataContext is StyleItemViewModel)
            {
                if (!((StyleItemViewModel)((Grid)sender).DataContext).StyleItem.CanRename)
                {
                    return;
                }
            }

            InitializeDBClickTimer();
            if (e.ChangedButton == MouseButton.Left)
            {
                rootNodeMouseDownCount++;
            }
            if (!rootNodeClickTimer.IsEnabled)
            {
                rootNodeClickTimer.Start();
            }
        }
    }
}