using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Exceptions;

namespace BossAuras.Core.Models;

/// <summary>
/// The ModelDb for AuraModels.
/// </summary>
public static class AuraDb
{
    // _contentById Maps every Model from its ModelId to its AbstractModel. We only use it in the copy of ModelDb methods (see below)
    #region Accessors
    
    private static readonly FieldInfo? ContentByIdField = typeof(ModelDb).GetField(
                "_contentById", BindingFlags.NonPublic | BindingFlags.Static);
    
    /// <summary>
    /// Reference to ModelDb._contentById
    /// </summary>
    private static readonly Dictionary<ModelId, AbstractModel> ContentByIdRef = 
                (Dictionary<ModelId, AbstractModel>)ContentByIdField?.GetValue(null)! ?? throw new InvalidOperationException();
    
    #endregion Accessors
    
    private static IEnumerable<AuraModel>? _allAuras;
    public static IEnumerable<AuraModel> AllAuras => _allAuras ??= from t in ModelDb.AllAbstractModelSubtypes
                where t.IsSubclassOf(typeof(AuraModel)) select (AuraModel)Get(t);


    
    
    // Carbon copy of private ModelDb.Get<t> with cast to AuraModel. Similar to ModelDbs "Power<T>", "Event<T>", etc.
    public static AuraModel Aura<T>() where T : AuraModel
    {
        return (T)ContentByIdRef[ModelDb.GetId<T>()];
    }
    
    // Carbon copy of private ModelDb.Get(Type type).
    private static AbstractModel Get(Type type)
    {
        if (!type.IsSubclassOf(typeof(AbstractModel)))
            throw new InvalidOperationException();
        
        var id = ModelDb.GetId(type);
        if (ContentByIdRef.TryGetValue(id, out var value))
            return value;
        throw new ModelNotFoundException(id);
    }
}