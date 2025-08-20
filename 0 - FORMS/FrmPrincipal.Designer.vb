<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmPrincipal
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPrincipal))
        txtLog = New TextBox()
        ContextMenuStrip1 = New ContextMenuStrip(components)
        ABRIRToolStripMenuItem = New ToolStripMenuItem()
        ToolStripSeparator1 = New ToolStripSeparator()
        SAIRToolStripMenuItem = New ToolStripMenuItem()
        NotifyIcon1 = New NotifyIcon(components)
        TimerMonitoramento = New Timer(components)
        TxtEnderecoPP = New TextBox()
        Label1 = New Label()
        ContextMenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' txtLog
        ' 
        txtLog.Enabled = False
        txtLog.Location = New Point(0, -1)
        txtLog.Multiline = True
        txtLog.Name = "txtLog"
        txtLog.ScrollBars = ScrollBars.Vertical
        txtLog.Size = New Size(544, 511)
        txtLog.TabIndex = 1
        ' 
        ' ContextMenuStrip1
        ' 
        ContextMenuStrip1.Items.AddRange(New ToolStripItem() {ABRIRToolStripMenuItem, ToolStripSeparator1, SAIRToolStripMenuItem})
        ContextMenuStrip1.Name = "ContextMenuStrip1"
        ContextMenuStrip1.Size = New Size(172, 54)
        ' 
        ' ABRIRToolStripMenuItem
        ' 
        ABRIRToolStripMenuItem.Name = "ABRIRToolStripMenuItem"
        ABRIRToolStripMenuItem.Size = New Size(171, 22)
        ABRIRToolStripMenuItem.Text = "ABRIR O SIGMA-Q"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(168, 6)
        ' 
        ' SAIRToolStripMenuItem
        ' 
        SAIRToolStripMenuItem.Name = "SAIRToolStripMenuItem"
        SAIRToolStripMenuItem.Size = New Size(171, 22)
        SAIRToolStripMenuItem.Text = "SAIR"
        ' 
        ' NotifyIcon1
        ' 
        NotifyIcon1.Icon = CType(resources.GetObject("NotifyIcon1.Icon"), Icon)
        NotifyIcon1.Text = "NotifyIcon1"
        NotifyIcon1.Visible = True
        ' 
        ' TimerMonitoramento
        ' 
        TimerMonitoramento.Enabled = True
        TimerMonitoramento.Interval = 15000
        ' 
        ' TxtEnderecoPP
        ' 
        TxtEnderecoPP.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TxtEnderecoPP.Location = New Point(550, 50)
        TxtEnderecoPP.Name = "TxtEnderecoPP"
        TxtEnderecoPP.Size = New Size(436, 29)
        TxtEnderecoPP.TabIndex = 2
        TxtEnderecoPP.Text = "C:\PisoPlanta\EXPORT"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(550, 26)
        Label1.Name = "Label1"
        Label1.Size = New Size(250, 21)
        Label1.TabIndex = 3
        Label1.Text = "CAMINHO DO PISO DE PLANTA:"
        ' 
        ' FrmPrincipal
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1059, 515)
        Controls.Add(Label1)
        Controls.Add(TxtEnderecoPP)
        Controls.Add(txtLog)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "FrmPrincipal"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Sigma-Q - Sistema de Gestão de Dados"
        ContextMenuStrip1.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents txtLog As TextBox
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents ABRIRToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents SAIRToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents TimerMonitoramento As Timer
    Friend WithEvents TxtEnderecoPP As TextBox
    Friend WithEvents Label1 As Label

End Class
