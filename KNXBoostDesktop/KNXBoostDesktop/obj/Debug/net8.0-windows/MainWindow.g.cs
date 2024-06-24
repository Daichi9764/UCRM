﻿#pragma checksum "..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5CB47D38E77791187BC99802F6E0FA67DEA19210"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using KNXBoostDesktop;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace KNXBoostDesktop {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 131 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ImportProjectButton;
        
        #line default
        #line hidden
        
        
        #line 132 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OpenConsoleButton;
        
        #line default
        #line hidden
        
        
        #line 135 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OpenGroupAddressFileButton;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ExportModifiedProjectButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.6.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/KNXBoostDesktop;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.6.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 10 "..\..\..\MainWindow.xaml"
            ((KNXBoostDesktop.MainWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.ClosingMainWindow);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 101 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).SelectedItemChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.AdressesDeGroupesOriginales_SelectedItemChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 113 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).SelectedItemChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.AdressesDeGroupesModifiées_SelectedItemChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ImportProjectButton = ((System.Windows.Controls.Button)(target));
            
            #line 131 "..\..\..\MainWindow.xaml"
            this.ImportProjectButton.Click += new System.Windows.RoutedEventHandler(this.ImportProjectButtonClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.OpenConsoleButton = ((System.Windows.Controls.Button)(target));
            
            #line 132 "..\..\..\MainWindow.xaml"
            this.OpenConsoleButton.Click += new System.Windows.RoutedEventHandler(this.OpenConsoleButtonClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.OpenGroupAddressFileButton = ((System.Windows.Controls.Button)(target));
            
            #line 135 "..\..\..\MainWindow.xaml"
            this.OpenGroupAddressFileButton.Click += new System.Windows.RoutedEventHandler(this.OpenGroupAddressFileButtonClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.ExportModifiedProjectButton = ((System.Windows.Controls.Button)(target));
            
            #line 136 "..\..\..\MainWindow.xaml"
            this.ExportModifiedProjectButton.Click += new System.Windows.RoutedEventHandler(this.ExportModifiedProjectButtonClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

