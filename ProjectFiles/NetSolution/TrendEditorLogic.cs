#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.SQLiteStore;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.Store;
using FTOptix.S7TiaProfinet;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using System.Collections.Generic;
#endregion

public class TrendEditorLogic : BaseNetLogic
{
    Trend handledTrend;
    TrendPen editPen;
    TrendThreshold editThreshold;
    ListBox thresholdList;

    public override void Start()
    {
        handledTrend = Owner.Get<Trend>("HandledTrend");
        thresholdList = InformationModel.Get(LogicObject.GetVariable("ThresholdList").Value) as ListBox;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void AddUpdatePen()
    {
        string penBrowseName = LogicObject.GetVariable("PenName").Value;
        string penActualBrowseName = LogicObject.GetVariable("PenActualName").Value;
        if (string.IsNullOrEmpty(penBrowseName)) return;
        if (editPen == null) editPen = handledTrend.Pens.Get(penBrowseName);
        IUAVariable penColorVariable = LogicObject.GetVariable("ColorPen");
        IUAVariable penThicknessVariable = LogicObject.GetVariable("PenThickness");
        IUAVariable penEnabledVariable = LogicObject.GetVariable("PenEnable");
        IUAVariable destinationModel = InformationModel.GetVariable(LogicObject.GetVariable("PenValueNodeId").Value);
        if (handledTrend.Pens.Get(penActualBrowseName) != null && penActualBrowseName != penBrowseName)
        {
            editPen = handledTrend.Pens.Get(penActualBrowseName);
            editPen.BrowseName = penBrowseName;
            editPen.Title = new LocalizedText(penBrowseName, penBrowseName, Session.ActualLocaleId);
        }
        if (editPen == null)
        {
            editPen = InformationModel.MakeVariable<TrendPen>(penBrowseName, OpcUa.DataTypes.BaseDataType);
            editPen.Title = new LocalizedText(penBrowseName, penBrowseName, Session.ActualLocaleId);        
        }
        if (!handledTrend.Pens.Contains(editPen)) handledTrend.Pens.Add(editPen);
        if (penColorVariable.Value != editPen.Color) editPen.Color = penColorVariable.Value;
        if (penThicknessVariable.Value != editPen.Thickness) editPen.Thickness = penThicknessVariable.Value;
        if (penEnabledVariable.Value != editPen.Enabled) editPen.Enabled = penEnabledVariable.Value;
        editPen.ResetDynamicLink();
        editPen.SetDynamicLink(destinationModel);       
        penColorVariable.Value = System.Drawing.Color.Black.ToArgb();
        LogicObject.GetVariable("PenName").Value = string.Empty;
        LogicObject.GetVariable("PenActualName").Value = string.Empty;
        penEnabledVariable.Value = false;
        penThicknessVariable.Value = -1;        
        thresholdList.ModelVariable.ResetDynamicLink();
        thresholdList.Refresh();
        editPen = null;
    }

    [ExportMethod]
    public void CheckAdd()
    {
        string penBrowseName = LogicObject.GetVariable("PenName").Value;
        string penActualBrowseName = LogicObject.GetVariable("PenActualName").Value;
        if (handledTrend.Pens.Get(penBrowseName) == null && handledTrend.Pens.Get(penActualBrowseName) == null)
        {
            editPen = InformationModel.MakeVariable<TrendPen>(penBrowseName, OpcUa.DataTypes.BaseDataType);
            thresholdList.ModelVariable.ResetDynamicLink();
            thresholdList.Model = editPen.GetObject("Thresholds").NodeId;
            thresholdList.Refresh();
        }         
    }

    [ExportMethod]
    public void CheckThresholdAdd()
    {
        string thresholdName = LogicObject.GetVariable("ThresholdName").Value;
        string thresholdActualBrowseName = LogicObject.GetVariable("ThresholdActualName").Value;
        if (editPen.Get(thresholdName) == null && editPen.Get(thresholdActualBrowseName) == null)
        {
            editThreshold = InformationModel.MakeObject<TrendThreshold>(thresholdName);            
        }
    }

    [ExportMethod]
    public void AddUpdateThresholds()
    {
        string thresholdName = LogicObject.GetVariable("ThresholdName").Value;
        string thresholdActualBrowseName = LogicObject.GetVariable("ThresholdActualName").Value;
        if (string.IsNullOrEmpty(thresholdName)) return;
        if (editThreshold == null) editThreshold = editPen.Thresholds.Get(thresholdName);
        IUAVariable thresholdColorVariable = LogicObject.GetVariable("ColorPen");
        IUAVariable thresholdThicknessVariable = LogicObject.GetVariable("ThresholdThickness");
        IUAVariable thresholdValueVariable = LogicObject.GetVariable("ThresholdValue");
        if (editPen.Thresholds.Get(thresholdActualBrowseName) != null && thresholdActualBrowseName != thresholdName)
        {
            editThreshold = editPen.Thresholds.Get(thresholdActualBrowseName);
            editThreshold.BrowseName = thresholdName;
        }
        if (editThreshold == null)
        {
            editThreshold = InformationModel.MakeObject<TrendThreshold>(thresholdName);
        }
        if (!editPen.Thresholds.Contains(editThreshold)) editPen.Thresholds.Add(editThreshold);
        if (thresholdColorVariable.Value != editThreshold.Color) editThreshold.Color = thresholdColorVariable.Value;
        if (thresholdThicknessVariable.Value != editThreshold.Thickness) editThreshold.Thickness = thresholdThicknessVariable.Value;
        if (thresholdValueVariable.Value != editThreshold.Value) editThreshold.Value = thresholdValueVariable.Value;
        LogicObject.GetVariable("ColorPen").Value = editPen.Color;
        LogicObject.GetVariable("ThresholdName").Value = string.Empty;
        LogicObject.GetVariable("ThresholdThickness").Value = -1;
        LogicObject.GetVariable("ThresholdValue").Value = 0;
        editThreshold = null;
    }

    [ExportMethod]
    public void RemovePen(NodeId penToRemove)
    {
        handledTrend.Pens.Remove(InformationModel.Get<TrendPen>(penToRemove));   
    }

    [ExportMethod]
    public void RemoveThreshold(NodeId thresholdToRemove)
    {
        editPen.Thresholds.Remove(InformationModel.Get<TrendThreshold>(thresholdToRemove));
    }

    [ExportMethod]
    public void SetReferencedValue(NodeId comboBox, NodeId penToEdit) 
    {
        ComboBox comboBoxList = InformationModel.Get<ComboBox>(comboBox);
        editPen = InformationModel.Get<TrendPen>(penToEdit);
        DynamicLink link = editPen?.Children.GetVariable("DynamicLink") as DynamicLink;
        if (link != null) 
        {
            PathResolverResult resolvePathResult = LogicObject.Context.ResolvePath(editPen, link?.Value);
            if (resolvePathResult != null && comboBoxList != null) comboBoxList.SelectedValue = resolvePathResult.ResolvedNode.NodeId;
        }        
        if (editPen != null)
        {
            LogicObject.GetVariable("PenActualName").Value = editPen.BrowseName;
            LogicObject.GetVariable("PenName").Value = editPen.BrowseName;
            LogicObject.GetVariable("ColorPen").Value = editPen.Color;
            LogicObject.GetVariable("PenThickness").Value = editPen.Thickness;
            LogicObject.GetVariable("PenEnable").Value = editPen.Enabled;
            thresholdList.Model = editPen.GetObject("Thresholds").NodeId;
            thresholdList.Refresh();
        }
    }

    [ExportMethod]
    public void SetThresholdReference(NodeId thresholdToEdit)
    {
        editThreshold = InformationModel.Get<TrendThreshold>(thresholdToEdit);
        if (editThreshold != null)
        {
            LogicObject.GetVariable("ThresholdName").Value = editThreshold.BrowseName;
            LogicObject.GetVariable("ThresholdActualName").Value = editThreshold.BrowseName;
            LogicObject.GetVariable("ColorPen").Value = editThreshold.Color;
            LogicObject.GetVariable("ThresholdThickness").Value = editThreshold.Thickness;
            LogicObject.GetVariable("ThresholdValue").Value = editThreshold.Value;
        }
    }
}
