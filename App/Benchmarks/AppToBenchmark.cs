using System;
using SqlReflect;
using SqlReflectTest.DataMappers;
using SqlReflectTest.Model;

public static class AppToBenchmark
{
    static readonly string NORTHWND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

    private static readonly String[] GREEK_CHARS = new String[]
    {
        "Alpha", "Beta", "Gamma", "Delta", "Epsilon",
        "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda",
        "Mu", "Nu", "Xi", "Omicron", "Pi", "Rho", "Sigma",
        "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega"
    };

    public static Object noJoin()
    {
        return null;
    }

    private static readonly IDataMapper customerHardCodedMapper = new CustomerDataMapper(typeof(Customer), NORTHWND, false);


    public static Object testDataMapper() {
        return customerHardCodedMapper.GetAll();
    }

    public static Object testReflectDataMapper()
    {
        return new ReflectDataMapper(typeof(Customer), NORTHWND, false);
    }

    public static Object testEmitDataMapper()
    {
        return EmitDataMapper.Build(typeof(Customer), NORTHWND, false);
    }

    public static void Main()
    {
        const long ITER_TIME = 1000;
        const long NUM_WARMUP = 10;
        const long NUM_ITER = 10;

        NBench.Benchmark(new BenchmarkMethod(AppToBenchmark.testDataMapper), "testDataMapper", ITER_TIME, NUM_WARMUP, NUM_ITER);
        //NBench.Benchmark(new BenchmarkMethod(AppToBenchmark.testReflectDataMapper), "testReflectDataMapper", ITER_TIME, NUM_WARMUP, NUM_ITER);
        //NBench.Benchmark(new BenchmarkMethod(AppToBenchmark.testEmitDataMapper), "testEmitDataMapper", ITER_TIME, NUM_WARMUP, NUM_ITER);
    }
}

