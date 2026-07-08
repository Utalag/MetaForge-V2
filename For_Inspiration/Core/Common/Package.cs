using MetaForge.Core.Abstractions;
using MetaForge.Core.Common;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Common;

/// <summary>
/// Package - ucelená softwarová síť vycházející z jednoho kořene (DomainModel).
/// Implementuje koncept "The Branching" - z kořenového modelu se kód rozbíhá do specializovaných vrstev.
/// </summary>
public class Package : RootElement
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private Class? _domainModel;
    private int _totalCreditScore;

    /// <summary>
    /// Název balíčku.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Popis balíčku.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Kořenový doménový model (Single Source of Truth).
    /// </summary>
    public Class? DomainModel
    {
        get => _domainModel;
        set
        {
            if (_domainModel != value)
            {
                _domainModel = value;
                OnPropertyChanged();
                RecalculateCreditScore();
            }
        }
    }

    /// <summary>
    /// Celkové kreditové skóre balíčku.
    /// </summary>
    public int TotalCreditScore
    {
        get => _totalCreditScore;
        private set
        {
            if (_totalCreditScore != value)
            {
                _totalCreditScore = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Vrstvy generované z doménového modelu.
    /// </summary>
    public Dictionary<LayerType, List<Class>> Layers { get; } = new()
    {
        [LayerType.Domain] = new(),
        [LayerType.Database] = new(),
        [LayerType.Contract] = new(),
        [LayerType.Service] = new(),
        [LayerType.Api] = new()
    };

    /// <summary>
    /// Přepočítá celkové kreditové skóre balíčku.
    /// </summary>
    public void RecalculateCreditScore()
    {
        var total = 0;

        // Přidej kredity z doménového modelu
        if (DomainModel != null)
        {
            total += DomainModel.CalculateTotalCreditScore();
        }

        // Přidej kredity ze všech vrstev
        foreach (var layer in Layers.Values)
        {
            foreach (var cls in layer)
            {
                total += cls.CalculateTotalCreditScore();
            }
        }

        TotalCreditScore = total;
    }

    /// <summary>
    /// Přidá třídu do konkrétní vrstvy.
    /// </summary>
    public void AddToLayer(LayerType layerType, Class cls)
    {
        Layers[layerType].Add(cls);
        RecalculateCreditScore();
    }

    /// <summary>
    /// Zkontroluje, zda je balíček připraven k exportu (Zero-Fault).
    /// </summary>
    public bool CanExport()
    {
        // Zkontroluj doménový model
        if (DomainModel == null || DomainModel.HasErrors())
        {
            return false;
        }

        // Zkontroluj všechny vrstvy
        foreach (var layer in Layers.Values)
        {
            foreach (var cls in layer)
            {
                if (cls.HasErrors())
                {
                    return false;
                }
            }
        }

        return true;
    }
}
