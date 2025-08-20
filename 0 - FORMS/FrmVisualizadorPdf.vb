Imports PdfiumViewer

Public Class FrmVisualizadorPdf
    Private viewer As PdfViewer

    Public Sub New()
        InitializeComponent()

        ' Cria o componente PDF viewer dinamicamente
        viewer = New PdfViewer()
        viewer.Dock = DockStyle.Fill
        Me.Controls.Add(viewer)
    End Sub

    Public Sub CarregarPDF(caminho As String)
        Dim doc = PdfDocument.Load(caminho)
        viewer.Document = doc
    End Sub
End Class
