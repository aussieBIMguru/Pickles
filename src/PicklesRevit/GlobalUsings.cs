// This file provides alias' and availability of namespaces in all files

// Dynamo element alias'
global using DynElement = global::Revit.Elements.Element;
global using DynRevision = global::Revit.Elements.Revision;
global using DynView = global::Revit.Elements.Views.View;
global using DynSheet = global::Revit.Elements.Views.Sheet;
global using DynFamilySymbol = global::Revit.Elements.FamilyType;

// Revit database alias
global using DB = Autodesk.Revit.DB;

// Pickle usings
global using Pickles.Enums;
global using Pickles.Extensions;
global using pklGen = Pickle.Utilities.Util_General;