   M S B A M L   `   `   `  ����E  AModbusPlug, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ModbusPlug
ModbusPlug  5   	       ModbusPlug.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:ModbusPlug     5   	     � Title$  Modbus Plug��5   	   $	��300��6   $	��600��6*   $	��400��5	   	   $	��600��6    � Cursor$  ��6(   .��5      � $��10q�6   $  ��6    � ColumnDefinitions 5   
   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��0��6"    �� Orientation$ Vertical=�62   $
��Left=�6I   $��0,30,0,0q�6d   .+�5      �� -   5        $��RunServerButton��5      $��
Run Server��6,      ControlButtonsStyle#��[  5      5      �� 5!      -   5        $��StartClientReadingButton��5!      $��Start Client Reading��65   #��[  5"      5#      5$      �� 5%   
   $��0��6   $��1��6"   $ Vertical=�62   $
��Left=�6I   $��0,30,0,0q�6d   .+�5&      �� -   5        $��ConfigServer��5&      $��Stop Server��6)   #��[  5'      $
 Hand��5(   -   �� 5*      -   5        $��DisconectClients��5*      $��Stop Client Reading��6-   #��[  5+      5,      5-      �� 5/   
   $��0��6   $��2��6"   $ Vertical=�62   $
��Left=�6I   $��0,30,0,0q�6d   .+�50      �� -   5        $��a��50      $��Config Server��6   #��[  51      $
 Hand��52   -   �� 53      -   5        $��b��53      $��Config Client Reading��6   #��[  54      55      56      �� 5:   
   -   5        $��TextBoxLogs��5:   F   $��1��6   " �� VerticalScrollBarVisibility$
 Auto=�6#   $��10,5,0,0q�5;       �� 
IsReadOnly$
 True��6$   $  ��66   $��3��6@   $	��Top=�6T   5<      5=      