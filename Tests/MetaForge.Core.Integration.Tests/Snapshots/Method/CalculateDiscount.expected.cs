




public static class Pricing
{




    
    public static decimal CalculateDiscount(decimal price, decimal percent)
    {
        if ((percent > 100))
            percent = 100;
        else
        {
                if ((percent < 0))
                    percent = 0;
            }
        return (price * (percent / 100));
    }
    

}

