﻿

#pragma checksum "C:\Users\fuerte\Dropbox\projects\Windows8_Projects\FlurrysticksW8\Flurrysticks\AccountApplicationsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EB04125CF4F90DA6E84384DEB08469BE"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Flurrystics
{
    partial class AccountApplicationsPage : global::Flurrystics.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 99 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.itemGridView_SelectionChanged;
                 #line default
                 #line hidden
                #line 106 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.ListViewBase)(target)).ItemClick += this.ItemView_ItemClick;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 189 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.itemListView_SelectionChanged;
                 #line default
                 #line hidden
                #line 193 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.ListViewBase)(target)).ItemClick += this.ItemView_ItemClick;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 218 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.GoBack;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 252 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.headerMenuClicked;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 163 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.addClick_Click_1;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 164 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.cancelClick_Click_1;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 276 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Click;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 278 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Click;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 284 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.AppBar)(target)).Opened += this.bottomAppBar_Opened_1;
                 #line default
                 #line hidden
                #line 284 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.AppBar)(target)).Closed += this.bottomAppBar_Closed;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 291 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.addToFavButton_Click;
                 #line default
                 #line hidden
                break;
            case 11:
                #line 292 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.clearSelection_Click;
                 #line default
                 #line hidden
                break;
            case 12:
                #line 293 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Click_3;
                 #line default
                 #line hidden
                break;
            case 13:
                #line 287 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Click_1;
                 #line default
                 #line hidden
                break;
            case 14:
                #line 288 "..\..\AccountApplicationsPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Click_2;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


