using LoginApp.Maui.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace LoginApp.Maui.Services
{
    public class LoginResponse
    {
        public string Status { get; set; }
        public UserResult Result { get; set; }
    }

    public class UserResult
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string TPD { get; set; }
        public string serie_doc { get; set; }
    }
    public class LoginService : ILoginService
    {
        public async Task<User> Login(string email, string password)
        {
            var client = new HttpClient();
            var data = new
            {
                usuario = email,
                password = password
            };
            var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            string url = "https://ventarapida-dms.000webhostapp.com/auth";
            HttpResponseMessage response = await client.PostAsync(url, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    LoginResponse loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (loginResponse == null || loginResponse.Status != "ok" || loginResponse.Result == null)
                    {
                        return null;
                    }

                    // Crear un objeto User asignando los valores desde UserResult
                    User user = new User
                    {
                        Id = loginResponse.Result.Id,
                        FullName = loginResponse.Result.FullName,
                        Email = loginResponse.Result.Email,
                        Password = loginResponse.Result.Password,
                        token = loginResponse.Result.Token,
                        TPD = loginResponse.Result.TPD,
                        serie_doc = loginResponse.Result.serie_doc,
                    };

                    if (user.Id == 0)
                    {
                        return null;
                    }

                    return user;
                }
                catch (JsonException)
                {
                    // Manejar excepción de deserialización JSON
                    return null;
                }
            }

            return null;
        }
    }
}
