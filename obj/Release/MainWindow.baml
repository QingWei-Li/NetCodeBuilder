   M S B A M L   `   `   `  ����   CodeBuilder    CodeBuilder.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  CodeBuilder by Cinwell��$	��500��$	��800�� � WindowStartupLocation$ CenterScreen=�.���  � RowDefinitions �� $��20 ��� $��20 ��� $��20 ��� $��20 ��� $��20 ��� �� $��20 �.+�� $��1�� � ColumnDefinitions �� $��20 ��� �� �� $��20 ��� $��60 ��� $��20 �.+��� -   $��	tbConnStr��$��1��$��2���� -   $��
btnConnect��$��4��$��连接��$��20�� �� 	IsDefault . � $��3�� �� $��20 ��� �� $��20 ��� $	��100 ��� $��20 ��� $	��100 ��� $��20 ��� $��50 ��� $��20 ��� �� $��5 ��� $��50 ��� $��20 �.+��� -   $��tbNamespace��$��1�� �� AutoWordSelection . �� -   $��cbTables��$��3��{�.  �� -   $��cbDAL��$��5��{�.  �� -   $��btnGenerateCode��$��7��$��生成��{�.  �� -   $��tbSelectPath��$��9���� -	   $��	btnExport��$��11��$��导出��{�.  � $��5�� �� $��20 ��� -
    �Name$ gridLeft���� $��20 ��� -   $ 	gridRight���� $��20 �.+��� -   $��tbModel��$��1��
�������$��双击复制Model�� �� 
IsReadOnly .  �� TextWrapping$
 Wrap=�"	 �� VerticalScrollBarVisibility$
	 Auto=�
 �� 	UndoLimit$
 0��.�� � ContextMenu �� .@�m� -   $��复制全部��.���� -   $��tbDAL��$��3��
�������$��双击复制DAL�� . $
 Wrap=�$
	 Auto=�$
 0��.�� �� .@�m� -   $��复制全部��.��