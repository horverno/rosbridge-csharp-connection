   M S B A M L   `   `   `  ����  
TurtleTest    TurtleTest.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Rosbridge turtle test��5   	   $	��480��6'   $	��480��67   $	��600��6D   .��5      �  � ColumnDefinitions 5   
   �� 5      $	��250 �6   �� 5      .+�5
   
   �� -   5         ��Name$ stackAll��5
      $��8q�6'   $��0��62   .+�5      �� -   5         ��Name$ txtIP��5      $��10.2.94.144��6%   $��2q�68   �� 5      -   5        $ txtPort��5      $
��9090��6'   $��2q�63   �� 5      -   5         ��Name$ 
btnConnect��5      $��Connect��6B   
��� E��$��2q�6k   �� 5      -   5        $ stackControls��5      $��8q�60   .+�5      �� -   5        $ btnSubscribe��5      $��Subcribe��6J   $��2q�6]   �� 5      -   5        $ 
btnForward��5      $��Forward��6F   $��2q�6X   �� 5      -   5        $ btnBackward��5      $��Backward��6H   $��2q�6[   �� 5      -	   5        $ btnLeft��5      $
��Left��6@   $��2q�6O   �� 5      -
   5        $ btnRight��5      $��Right��6B   $��2q�6R   �� 5      $��Length of velocity vector��6   �� 5      -   5        $ txtLgth��5      $��1��6+   $��2q�64   �� 5      $��Degree of rotation��6   $��2q�65   �� 5      -   5        $ txtTheta��5      $��90��6,   $��2q�66   �� 5      -   5         ��Name$ labelX��5      ��6)   �� *msg.x+�� 5      -   5        $ labelY��5      ��6(   �� *msg.y+�� 5      -   5        $ 
labelTheta��5      ��6-   �� *
receiver+5      5      �� 5   
   -   5         ��Name$ color_sensor_feedback��5      $��1��63   5       5!      5"      