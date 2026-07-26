// This file provides alias' and availability of namespaces in all files

// General Dynamo usings
global using Autodesk.DesignScript.Runtime;
global using Dynamo.Graph.Nodes;
global using DynamoServices;
global using Revit.Elements;
global using RevitServices.Persistence;
global using RevitServices.Transactions;

// Dynamo element alias'
global using DynCategory = global::Revit.Elements.Category;
global using DynCeiling = global::Revit.Elements.Ceiling;
global using DynCeilingType = global::Revit.Elements.CeilingType;
global using DynDocument = global::Revit.Application.Document;
global using DynElement = global::Revit.Elements.Element;
global using DynFamilyInstance = global::Revit.Elements.FamilyInstance;
global using DynFamilySymbol = global::Revit.Elements.FamilyType;
global using DynFloor = global::Revit.Elements.Floor;
global using DynFloorType = global::Revit.Elements.FloorType;
global using DynForgeType = global::Revit.Elements.ForgeType;
global using DynGroup = global::Revit.Elements.Group;
global using DynGroupType = global::Revit.Elements.GroupType;
global using DynLevel = global::Revit.Elements.Level;
global using DynRevision = global::Revit.Elements.Revision;
global using DynRoom = global::Revit.Elements.Room;
global using DynSheet = global::Revit.Elements.Views.Sheet;
global using DynSpecType = global::Revit.Elements.SpecType;
global using DynView = global::Revit.Elements.Views.View;
global using DynView3D = global::Revit.Elements.Views.View3D;
global using DynViewport = global::Revit.Elements.Viewport;
global using DynWarning = global::Revit.Application.Warning;
global using DynAdaptiveComponent = global::Revit.Elements.AdaptiveComponent;

// Dynamo geometry alias'
global using DynBb = global::Autodesk.DesignScript.Geometry.BoundingBox;
global using DynCoordinateSystem = global::Autodesk.DesignScript.Geometry.CoordinateSystem;
global using DynCurve = global::Autodesk.DesignScript.Geometry.Curve;
global using DynGeometry = global::Autodesk.DesignScript.Geometry.Geometry;
global using DynPlane = global::Autodesk.DesignScript.Geometry.Plane;
global using DynPoint = global::Autodesk.DesignScript.Geometry.Point;
global using DynPolyCurve = global::Autodesk.DesignScript.Geometry.PolyCurve;
global using DynSolid = global::Autodesk.DesignScript.Geometry.Solid;
global using DynSurface = global::Autodesk.DesignScript.Geometry.Surface;
global using DynVector = global::Autodesk.DesignScript.Geometry.Vector;

// Revit database alias
global using DB = Autodesk.Revit.DB;
global using RUI = Autodesk.Revit.UI;
global using Result = Autodesk.Revit.UI.Result;
global using DbSpace = Autodesk.Revit.DB.Mechanical.Space;

// Pickle usings
global using Pickles.Enums;
global using Pickles.Extensions;
global using Pickles.Helpers;
global using pklFrm = Pickles.Utilities.Utils_Forms;
global using pklCal = Pickles.Forms.Callers;
global using KeyedObject = Pickles.Forms.KeyedValue<object>;