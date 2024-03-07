namespace VolunteerAppSecurity
{
    public class Constants
    {
        public const string SenderEmail = "nalivaykodiana@gmail.com";
        public const string Password = "nrea scax mggc asvd";
        // ngrok http https://localhost:7299/ --host-header="localhost:7299"
        public static string ngrok = "https://bd5f-178-212-241-227.ngrok-free.app";
        public static string SuccessMessage = @"<!DOCTYPE html>
                <html lang=""uk"">
                <head>
                  <meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <title><3</title>
                </head>
                <body>
                  <h1><3</h1>
                </body>
                </html>";

        public static string FailureMessage = @"<!DOCTYPE html>
                <html lang=""uk"">
                <head>
                  <meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <title>Confirmation email</title>
                </head>
                <body>
                  <h1>Unfortunately, email confirmation did not happen!</h1>
                  <p>We could not verify your email address..</p>
                  <p>Try again later.</p>
                </body>
                </html>";
    
    }
}
