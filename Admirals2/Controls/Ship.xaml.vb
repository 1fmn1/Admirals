Imports System.Globalization
Imports Microsoft.Sample.Controls

Public Class Ship
    'Inherits ShipsData
    Implements IVirtualChild
    Dim _visual As Canvas
    Dim _bounds As Rect
    'Dim _gdata As ShipsData = New ShipsData()
    Public Shared Property GdataProperty As DependencyProperty = DependencyProperty.Register("gdata",
                       GetType(GalaxyData), GetType(Ship),
                       New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.AffectsMeasure Or
                                    FrameworkPropertyMetadataOptions.AffectsRender))
    Public Property gdata As GalaxyData
        Get
            Return CType(GetValue(GdataProperty), GalaxyData)
        End Get
        Set(ByVal value As GalaxyData)
            SetValue(GdataProperty, value)
        End Set
    End Property
    'Dim _gdata As New ShipsData
    Public Sub New()

        ' Этот вызов является обязательным для конструктора.
        InitializeComponent()

        ' Добавить код инициализации после вызова InitializeComponent().

    End Sub
    Public Sub New(a As GalaxyData)
        ' Этот вызов является обязательным для конструктора.
        Me.DataContext = a
        gdata = a
        InitializeComponent()
        ImageHolder.Stroke = Nothing
        ClanHolder.Stroke = Nothing
        ColorHolder.Stroke = Nothing
        _bounds = New Rect(New Point(a.x + 1700 / 2 - 20, -a.y + 1300 / 2 - 20), New Size(20, 20))
        ' Добавить д инициализации после вызова InitializeComponent().
    End Sub



























    Public ReadOnly Property Bounds As Rect Implements IVirtualChild.Bounds
        Get
            Return _bounds
        End Get
    End Property

    Public ReadOnly Property Visual As UIElement Implements IVirtualChild.Visual
        Get
            Return _visual
        End Get
    End Property

    Public Event BoundsChanged As EventHandler Implements IVirtualChild.BoundsChanged

    Public Sub DisposeVisual() Implements IVirtualChild.DisposeVisual
        _visual = Nothing
    End Sub

    Public Function CreateVisual(parent As VirtualCanvas) As UIElement Implements IVirtualChild.CreateVisual
        If _visual Is Nothing Then
            _visual = New Canvas
            _visual.Children.Add(ui)
            Return _visual
        Else
            Return _visual
        End If
    End Function
End Class
