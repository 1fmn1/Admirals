Imports Microsoft.Sample.Controls

Public Class VirtualChild
    Implements IVirtualChild
    Dim _visual As Canvas
    Dim _visual2 As Canvas
    Dim _bounds As Rect
    Sub New(ByRef Visual As Canvas, ByVal b As Rect)
        _visual2 = Visual
        _bounds = b
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

    Public Function CreateVisual(parent As Microsoft.Sample.Controls.VirtualCanvas) As UIElement Implements IVirtualChild.CreateVisual
        If _visual Is Nothing Then
            _visual = New Canvas
            _visual = _visual2
            Return _visual
        Else
            Return _visual
        End If
    End Function
End Class
