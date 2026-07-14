// This file provides alias' and availability of namespaces in all files

// General Dynamo usings
global using Dynamo.Graph.Nodes;
global using Autodesk.DesignScript.Runtime;
global using RevitServices.Persistence;
global using DynamoServices;
global using RevitServices.Transactions;
global using Revit.Elements;

// Dynamo element alias'
global using DynElement = global::Revit.Elements.Element;
global using DynDocument = global::Revit.Application.Document;
global using DynWarning = global::Revit.Application.Warning;
global using DynRevision = global::Revit.Elements.Revision;
global using DynView = global::Revit.Elements.Views.View;
global using DynSheet = global::Revit.Elements.Views.Sheet;
global using DynGroup = global::Revit.Elements.Group;
global using DynFamilySymbol = global::Revit.Elements.FamilyType;
global using DynFamilyInstance = global::Revit.Elements.FamilyInstance;
global using DynSpecType = global::Revit.Elements.SpecType;
global using DynGroupType = global::Revit.Elements.GroupType;
global using DynForgeType = global::Revit.Elements.ForgeType;
global using DynCategory = global::Revit.Elements.Category;

// Dynamo geometry alias'
global using DynGeometry = global::Autodesk.DesignScript.Geometry.Geometry;
global using DynPoint = global::Autodesk.DesignScript.Geometry.Point;
global using DynVector = global::Autodesk.DesignScript.Geometry.Vector;
global using DynSolid = global::Autodesk.DesignScript.Geometry.Solid;
global using DynCurve = global::Autodesk.DesignScript.Geometry.Curve;
global using DynBb = global::Autodesk.DesignScript.Geometry.BoundingBox;
global using DynPlane = global::Autodesk.DesignScript.Geometry.Plane;
global using DynPolyCurve = global::Autodesk.DesignScript.Geometry.PolyCurve;
global using DynSurface = global::Autodesk.DesignScript.Geometry.Surface;
global using DynCoordinateSystem = global::Autodesk.DesignScript.Geometry.CoordinateSystem;

// Revit database alias
global using DB = Autodesk.Revit.DB;
global using RUI = Autodesk.Revit.UI;
global using Result = Autodesk.Revit.UI.Result;

// Pickle usings
global using Pickles.Enums;
global using Pickles.Extensions;
global using Pickles.Helpers;
global using pklFrm = Pickles.Utilities.Utils_Forms;
global using pklCal = Pickles.Forms.Callers;
global using KeyedObject = Pickles.Forms.KeyedValue<object>;
global using pklEnum = Pickles.Enums.EnumHelpers;