using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnnotationDocument : AnnotationBase
{

    /// <summary>
    /// Das Wörterbuch in dem alle Kindelemente nach ID einsortiert werden.
    /// </summary>
    public Dictionary<int, AnnotationBase> Text_ID_Map { get; private set; }

    /// <summary>
    /// Das Wörterbuch in dem alle Kindelemente nach Typ einsortiert werden.
    /// </summary>
    public Dictionary<Type, List<AnnotationBase>> Type_Map { get; private set; }

    /// <summary>
    /// Das Wörterbuch in dem alle Kindelemente nach Anfangsindex einsortiert werden.
    /// </summary>
    public Dictionary<int, HashSet<AnnotationBase>> Begin_Map { get; private set; }

    /// <summary>
    /// Das Wörterbuch in dem alle Kindelemente nach Abschlussindex einsortiert werden.
    /// </summary>
    public Dictionary<int, HashSet<AnnotationBase>> End_Map { get; private set; }

    /// <summary>
    /// Gibt an, ob das Dokument geändert wurde.
    /// </summary>
    public bool HasChanges;

    /// <summary>
    /// Gibt an, ob in dem Dokument PartOfSpeech-Klasseninstanzen vorhanden sind.
    /// </summary>
    public bool HasPOS => POSCount > 0;

    /// <summary>
    /// Gibt die Anzahl der im Dokument vorkommenden PartOfSpeech-Klasseninstanzen an.
    /// </summary>
    public int POSCount { get; private set; }
    
    /// <summary>
    /// Alle Kapitel des Dokuments.
    /// </summary>
    public IEnumerable<Chapter> Chapters
    {
        get { return ChildElements as IEnumerable<Chapter>; }
    }

    public delegate void OnCommandStackChange();

    public OnCommandStackChange OnCanUndo;
    private bool _canUndo;
    public bool CanUndo
    {
        get { return _canUndo; }
        set
        {
            if (value == _canUndo) return;
            _canUndo = value;
            OnCanUndo?.Invoke();
        }
    }

    public OnCommandStackChange OnCanRedo;
    private bool _canRedo;
    public bool CanRedo
    {
        get { return _canRedo; }
        set
        {
            if (value == _canRedo) return;
            _canRedo = value;
            OnCanRedo?.Invoke();
        }
    }


    /// <summary>Der Konstruktor der AnnotationDocument-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer des Dokuments.</param>
    /// <param name="begin">Der Anfangsindex des Dokuments im Text.</param>
    /// <param name="end">Der Abschlussindex des Dokuments im Text.</param>
    public AnnotationDocument(int id, string text) : base(id, 0, text.Length, AnnotationTypes.DOCUMENT, null)
    {
        Text_ID_Map = new Dictionary<int, AnnotationBase>();
        Type_Map = new Dictionary<Type, List<AnnotationBase>>();
        Begin_Map = new Dictionary<int, HashSet<AnnotationBase>>();
        End_Map = new Dictionary<int, HashSet<AnnotationBase>>();
        _textContent = text;
        AddTextElement(this);
    }

    /// <summary>
    /// Diese Methode fügt ein beliebiges Textelement in das Dokument ein und erstellt die Wörterbücher-Einträge dafür. 
    /// </summary>
    /// <param name="data">Das Element, das eingefügt werden soll.</param>
    public void AddTextElement(AnnotationBase data)
    {
        //Debug.Log("AddTextElement: " + data.ID);
        Text_ID_Map.Add((int)data.ID, data);

        if (Begin_Map.ContainsKey(data.Begin))
            Begin_Map[data.Begin].Add(data);
        else
            Begin_Map.Add(data.Begin, new HashSet<AnnotationBase>() { data });

        if (End_Map.ContainsKey(data.End))
            End_Map[data.End].Add(data);
        else
            End_Map.Add(data.End, new HashSet<AnnotationBase>() { data });

        if (Type_Map.ContainsKey(data.GetType()))
            Type_Map[data.GetType()].Add(data);
        else
            Type_Map.Add(data.GetType(), new List<AnnotationBase>() { data });

        if (data is PartOfSpeech) POSCount += 1;
    }

    /// <summary>
    /// Diese Methode entfernt ein beliebiges Textelement aus dem Dokument ein und löscht alle Wörterbücher-Einträge dafür. 
    /// </summary>
    /// /// <param name="data">Das Element, das entfernt werden soll.</param>
    public void RemoveTextDataFromMaps(AnnotationBase data)
    {
        if (Begin_Map.ContainsKey(data.Begin))
            Begin_Map[data.Begin].Remove(data).ToString();

        if (End_Map.ContainsKey(data.End))
            End_Map[data.End].Remove(data).ToString();

        if (Text_ID_Map.ContainsKey((int)data.ID))
            Text_ID_Map.Remove((int)data.ID).ToString();

        if (Type_Map.ContainsKey(data.GetType()))
            Type_Map[data.GetType()].Remove(data);

    }

    /// <summary>Diese Methode gibt ein Elternelement des ausgewählten Typs zurück.</summary>
    /// <param name="type">Der Typ des gesuchten Elterns.</param>
    /// <param name="begin">Der Anfangsindex des Elements, dessen Eltern gesucht werden soll.</param>
    /// <param name="end">Der Abschlussindex des Elements, dessen Eltern gesucht werden soll.</param>
    public AnnotationBase GetParentOfType(string type, int begin, int end)
    {
        if (AnnotationTypes.TypeParentTable.ContainsKey(type))
        {
            foreach (AnnotationBase data in Type_Map[AnnotationTypes.TypeParentTable[type]])
                if (data.Begin <= begin && data.End >= end)
                    return data;
        }
        else
        {
            foreach (string parentType in AnnotationTypes.TypeParentTable.Keys)
            {
                if (parentType.Equals(type)) continue;
                foreach (AnnotationBase data in Type_Map[AnnotationTypes.TypeParentTable[parentType]])
                    if (data.Begin <= begin && data.End >= end)
                        return data;
            }
        }
        return null;
    }

    /// <summary>Diese Methode gibt das Element des ausgewählten Typs zurück, dessen Anfangsindex 
    /// kleiner-gleich und dessen Abschlussindex größer-gleich ist, als die angegebene Indices.</summary>
    /// <param name="begin">Der Anfangsindex.</param>
    /// <param name="end">Der Abschlussindex.</param>
    public AnnotationBaseType GetElementOfTypeInRangeGreaterEqual<AnnotationBaseType>(int begin, int end)
        where AnnotationBaseType : AnnotationBase
    {
        if (!Type_Map.ContainsKey(typeof(AnnotationBaseType))) return null;
        foreach (AnnotationBaseType data in Type_Map[typeof(AnnotationBaseType)])
            if (data.Begin <= begin && data.End >= end)
                return data;

        return null;
    }

    /// <summary>Diese Methode gibt ein Element des ausgewählten Typs zurück mit dem bestimmten Anfangs- und Abschlussindex zurück.</summary>
    /// <param name="begin">Der Anfangsindex des gesuchten Elements.</param>
    /// <param name="end">Der Abschlussindex des gesuchten Elements.</param>
    public bool ExistsElementOfTypeInRange<AnnotationBaseType>(int begin, int end, out AnnotationBaseType result)
        where AnnotationBaseType : AnnotationBase
    {
        foreach (AnnotationBaseType data in Type_Map[typeof(AnnotationBaseType)])
            if (data.Begin == begin && data.End == end)
            {
                result = data;
                return true;
            }
        result = null;
        return false;
    }

    /// <summary>Diese Methode gibt das erste Element des ausgewählten Typs im angegebenen Intervall zurück.</summary>
    /// <param name="begin">Der Anfangsindex des Intervalls.</param>
    /// <param name="end">Der Abschlussindex des Intervalls.</param>
    public AnnotationBaseType GetElementOfTypeAsChildOf<AnnotationBaseType>(int begin, int end)
        where AnnotationBaseType : AnnotationBase
    {
        foreach (AnnotationBaseType data in Type_Map[typeof(AnnotationBaseType)])
            if (data.Begin >= begin && data.End <= end)
                return data;

        return null;
    }

    /// <summary>Diese Methode gibt alle Elemente des ausgewählten Typs im angegebenen Intervall zurück.</summary>
    /// <param name="begin">Der Anfangsindex des Intervalls.</param>
    /// <param name="end">Der Abschlussindex des Intervalls.</param>
    public IEnumerable<AnnotationBaseType> GetElementsOfTypeInRange<AnnotationBaseType>(int begin, int end, bool includeSubclasses)
        where AnnotationBaseType : AnnotationBase
    {
        List<AnnotationBaseType> result = new List<AnnotationBaseType>();
        if (Type_Map.ContainsKey(typeof(AnnotationBaseType)))
        {
            foreach (AnnotationBaseType data in Type_Map[typeof(AnnotationBaseType)])
                if (data.Begin >= begin && data.End <= end)
                    result.Add(data);
        }        

        if (includeSubclasses)
        {
            Type baseType = typeof(AnnotationBaseType);
            List<Type> subtypes = new List<Type>(baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)));

            foreach (Type type in subtypes)
            {
                if (Type_Map.ContainsKey(type))
                {
                    for (int i = 0; i < Type_Map[type].Count; i++)
                        if (Type_Map[type][i].Begin >= begin &&
                            Type_Map[type][i].End <= end)
                            result.Add((AnnotationBaseType)Type_Map[type][i]);

                }
            }
        }
        return result;
    }

    /// <summary>Diese Methode gibt alle Elemente des ausgewählten Typs (Unterklassen inbegriffen) im angegebenen Intervall zurück.</summary>
    /// <param name="begin">Der Anfangsindex des Intervalls.</param>
    /// <param name="end">Der Abschlussindex des Intervalls.</param>
    /// <param name="includeSubclasses">Wenn True, werden auch vom angegebenen Typ erbende Klassen ebenfalls berücksichtigt.</param>
    public IEnumerable<AnnotationBaseType> GetElementsOfTypeFromTo<AnnotationBaseType>(int begin, int end, bool includeSubclasses)
        where AnnotationBaseType : AnnotationBase
    {
        List<AnnotationBaseType> result = new List<AnnotationBaseType>();
        if (Type_Map.ContainsKey(typeof(AnnotationBaseType)))
        {
            foreach (AnnotationBaseType data in Type_Map[typeof(AnnotationBaseType)])
                if (data.Begin == begin && data.End == end)
                    result.Add(data);
        }
        

        if (includeSubclasses)
        {
            Type baseType = typeof(AnnotationBaseType);
            List<Type> subtypes = new List<Type>(baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)));

            foreach (Type type in subtypes)
            {
                if (Type_Map.ContainsKey(type))
                {
                    for (int i = 0; i < Type_Map[type].Count; i++)
                        if (Type_Map[type][i].Begin == begin &&
                            Type_Map[type][i].End == end)
                            result.Add((AnnotationBaseType)Type_Map[type][i]);

                }
            }
        }
        
        return result;
    }

    /// <summary>Diese Methode gibt das erste Element des ausgewählten Typs zurück, das den Intervall des bestimmten Anfangs- und Abschlussindexes überdeckt.</summary>
    /// <param name="begin">Der Anfangsindex des Intervalls.</param>
    /// <param name="end">Der Abschlussindex des Intervalls.</param>
    public AnnotationBaseType GetElementOfTypeFromTo<AnnotationBaseType>(int begin, int end)
        where AnnotationBaseType : AnnotationBase
    {
        foreach (AnnotationBaseType data in Type_Map[typeof(AnnotationBaseType)])
            if (data.Begin == begin && data.End == end)
                return data;

        return null;
    }


    /// <summary>Diese Methode gibt ein Element mit einer der übergebenen Typen, dem bestimmten Anfangs- und Abschlussindexes zurück.</summary>
    /// <param name="types">Die Typen mit denen nach einem Element gesuchten werden soll.</param>
    /// <param name="begin">Der Anfangsindex des Intervalls.</param>
    /// <param name="end">Der Abschlussindex des Intervalls.</param>
    public AnnotationBase GetElementsOfTypes(Type[] types, int begin, int end)
    {
        foreach (Type type in types)
        {
            foreach (AnnotationBase data in Type_Map[type])
                if (data.Begin == begin && data.End == end)
                    return data;
        }


        return null;
    }


    public AnnotationBaseType GetElementByID<AnnotationBaseType>(int id, bool checkInheritingClasses)
        where AnnotationBaseType : AnnotationBase
    {
        if (!Text_ID_Map.ContainsKey(id)) return null;
        if (!checkInheritingClasses)
        {
            if (!Text_ID_Map[id].GetType().Equals(typeof(AnnotationBaseType))) return null;
            else return (AnnotationBaseType)Text_ID_Map[id];
        }
        else
        {
            Type baseType = typeof(AnnotationBaseType);
            HashSet<Type> subtypes = new HashSet<Type>(baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)));
            subtypes.Add(baseType);
            if (subtypes.Contains(Text_ID_Map[id].GetType())) return (AnnotationBaseType)Text_ID_Map[id];
            else return null;
        }
    }

    /// <summary>Diese Methode gibt alle Elemente des ausgewählten Typs zurück.</summary>
    public IEnumerable<AnnotationBaseType> GetElementsOfType<AnnotationBaseType>()
        where AnnotationBaseType : AnnotationBase
    {
        if (Type_Map.ContainsKey(typeof(AnnotationBaseType)))
            return Type_Map[typeof(AnnotationBaseType)].Cast<AnnotationBaseType>();

        return null;
    }

    public override void Actualize3DObject()
    {
        
    }

}
