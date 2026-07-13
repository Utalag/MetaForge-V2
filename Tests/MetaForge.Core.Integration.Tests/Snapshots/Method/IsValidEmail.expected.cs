




public static class Validators
{




    
    public static bool IsValidEmail(string email)
    {
        return ((string.IsNullOrWhiteSpace(email) == false) && email.Contains("@"));
    }
    

}

