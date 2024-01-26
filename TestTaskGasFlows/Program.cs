internal class Program
{
    private static void Main(string[] args)
    {
        Chemical[] components = {
            new Chemical("C1", .01604f, 1.303f),
            new Chemical("C2", .03007f, 1.188f),
            new Chemical("C3", .044097f, 1.127f),
            new Chemical("C4", .05812f, 1.092f),
            new Chemical("C5", .07215f, 1.074f),
            new Chemical("C6", .08617848f, 1.062f),
            new Chemical("C7", .100205f, 1.053f),
            new Chemical("N2", .0280134f, 1.4f),
            new Chemical("H2S", .034082f, 1.32f),
            new Chemical("CO2", .04401f, 1.28f),
            new Chemical("H2O", .01801528f, 1.33f)
        };

        float[] massFractions1 = { .87f, .07f, .01f, .01f, .01f, .015f, 0, 0, .005f, .005f, .005f };
        float[] massFractions2 = { .97f, .02f, .003f, .003f, .001f, .002f, 0, 0, .001f, 0, 0 };

        Gas gas1 = new Gas(components, massFractions1, 318, 4, 10);
        Gas gas2 = new Gas(components, massFractions2, 260, 6, 20);
        Gas mixture = new Gas(components, gas1, gas2);

        Console.WriteLine("Characteristics of mixture");
        mixture.ShowCharacteristics();
    }
}

static class Constants
{
    public const float GasConstant = 8.31446261815324f; // in J/(mol*K)
    public const float StandardAtmosphere = 101325; // in Pa
}

readonly struct Chemical
{
    public readonly string Name;
    public readonly float MolarMass; // in kg/mol
    public readonly float HeatCapacityRatio;

    public Chemical(string name, float molarMass, float heatCapacityRatio)
    {
        Name = name;
        MolarMass = molarMass;
        HeatCapacityRatio = heatCapacityRatio;
    }
}

class Gas
{
    private Chemical[] _components;
    private float[] _massFractions;
    public readonly float Temperature; // in K
    public readonly float Pressure; // in atm
    public readonly float MassFlux; // in kg/s
    public readonly float Density; // in kg/m^3
    public readonly float IsobaricHeatCapacity; // in J/(kg*K)
    public readonly float SpecificGasConstant; // in J/(kg*K)

    // constructor by characteristics
    public Gas(Chemical[] components, float[] massFractions, float temperature, float pressure, float massFlux)
    {
        _components = components;
        _massFractions = massFractions;
        IsobaricHeatCapacity = CalculateIsobaricHeatCapacity();
        SpecificGasConstant = CalculateSpecificGasConstant();
        Temperature = temperature;
        Pressure = pressure;
        MassFlux = massFlux;
        Density = Pressure * Constants.StandardAtmosphere / (Temperature * SpecificGasConstant);
    }

    private float CalculateIsobaricHeatCapacity()
    {
        float isobaricHeatCapacity = 0;
        float g, M, k;
        for (int i = 0; i < _components.Length; i++)
        {
            g = _massFractions[i];
            M = _components[i].MolarMass;
            k = _components[i].HeatCapacityRatio;
            isobaricHeatCapacity += g * Constants.GasConstant / M * k / (k - 1);
        }
        return isobaricHeatCapacity;
    }

    private float CalculateSpecificGasConstant()
    {
        float specificGasConstant = 0;
        float g, M;
        for (int i = 0; i < _components.Length; i++)
        {
            g = _massFractions[i];
            M = _components[i].MolarMass;
            specificGasConstant += g * Constants.GasConstant / M;
        }
        return specificGasConstant;
    }

    // constructor for mixture
    public Gas(Chemical[] components, Gas gas1, Gas gas2)
    {
        _components = components;
        _massFractions = CalculateMixtureMassFractions(gas1, gas2);
        IsobaricHeatCapacity = CalculateMixtureIsobaricHeatCapacity(gas1, gas2);
        SpecificGasConstant = CalculateMixtureSpecificGasConstant(gas1, gas2);
        Temperature = CalculateMixtureTemperature(gas1, gas2);
        Pressure = gas1.Pressure < gas2.Pressure ? gas1.Pressure : gas2.Pressure;
        MassFlux = gas1.MassFlux + gas2.MassFlux;
        Density = Pressure * Constants.StandardAtmosphere / (Temperature * SpecificGasConstant);
    }

    private float CalculateMixtureTemperature(Gas gas1, Gas gas2)
    {
        float T1 = gas1.Temperature, T2 = gas2.Temperature;
        float c1 = gas1.IsobaricHeatCapacity, c2 = gas2.IsobaricHeatCapacity;
        float Q1 = gas1.MassFlux, Q2 = gas2.MassFlux;
        return (Q1 * c1 * T1 + Q2 * c2 * T2) / (Q1 * c1 + Q2 * c2);
    }

    private float CalculateMixtureIsobaricHeatCapacity(Gas gas1, Gas gas2)
    {
        float c1 = gas1.IsobaricHeatCapacity, c2 = gas2.IsobaricHeatCapacity;
        float Q1 = gas1.MassFlux, Q2 = gas2.MassFlux;
        return (Q1 * c1 + Q2 * c2) / (Q1 + Q2);
    }

    private float CalculateMixtureSpecificGasConstant(Gas gas1, Gas gas2)
    {
        float R1 = gas1.SpecificGasConstant, R2 = gas2.SpecificGasConstant;
        float Q1 = gas1.MassFlux, Q2 = gas2.MassFlux;
        return (Q1 * R1 + Q2 * R2) / (Q1 + Q2);
    }

    private float[] CalculateMixtureMassFractions(Gas gas1, Gas gas2)
    {
        float Q1 = gas1.MassFlux, Q2 = gas2.MassFlux;
        float[] mixtureMassFractions = new float[_components.Length];
        for (int i = 0; i < _components.Length; i++)
        {
            mixtureMassFractions[i] = (Q1 * gas1.GetMassFraction(i) + Q2 * gas2.GetMassFraction(i)) / (Q1 + Q2);
        }
        return mixtureMassFractions;
    }

    public float GetMassFraction(int i)
    {
        if (i >= 0 && i < _components.Length)
        {
            return _massFractions[i];
        }
        else return 0;
    }

    public void ShowCharacteristics()
    {
        Console.WriteLine($"Temperature: {Temperature} K\nPressure: {Pressure} atm\n" +
            $"Mass flux: {MassFlux} kg/s\nDensity: {Density} kg/m^3\nComposition:");
        for (int i = 0; i < _components.Length; i++)
        {
            Console.WriteLine($"{_components[i].Name} - {_massFractions[i]}");
        }
    }
}

/*
Что можно улучшить/изменить:
1. Обработка некорректных параметров в конструкторах (разные размеры у _components и _massFractions в конструкторе
по характеристикам и разное содержимое у gas1._components и gas2._components в конструкторе для смеси). В этой задаче данные
известны и такой необходимости нет, но для более общего случая нужно предусмотреть.
2. Добавить возможность безопасного доступа к элементам Gas._components извне, чтобы не передавать их отдельным
аргументом в конструктор для смеси. Также можно хранить _components не в массиве, а в списке, что позволит обработать
исключение из п.1 (проходить по gas1._components и gas2._components и добавлять в список то, чего там еще нет, тогда списки
компонентов gas1 и gas2 могут различаться)
 */