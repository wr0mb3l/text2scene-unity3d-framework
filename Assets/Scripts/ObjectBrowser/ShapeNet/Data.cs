using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public string Name { get; protected set; }

    /// <summary>
    /// Die Identifikationsnummer.
    /// </summary>
    public object ID { get; protected set; }

    /// <summary>
    /// Enum zur Einordnung des Data Objekts.
    /// </summary>
    public enum SourceType { Local, Remote }

    /// <summary>
    /// Die Quellentyp, dem das Data-Objekt zugeornet wird.
    /// </summary>
    public SourceType Source;

    /// <summary>
    /// Alle untergeordnete GameObjects.
    /// </summary>
    public List<GameObject> ChildObjects;

    public DataContainer DataContainer;

    public virtual void SetupDataContainer(DataContainer container)
    {
        // base method does nothing
    }
}