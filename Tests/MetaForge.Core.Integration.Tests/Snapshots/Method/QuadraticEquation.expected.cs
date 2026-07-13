




public static class MathUtils
{




    
    public static string SolveQuadratic(double a, double b, double c)
    {
        discriminant = ((b * b) - ((4 * a) * c));
        if ((discriminant > 0))
            return "Two real roots";
        else
            if ((discriminant == 0))
                return "One real root";
            else
                return "Complex roots";
    }
    

}

