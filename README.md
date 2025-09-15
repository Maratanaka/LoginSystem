LoginSystem ASP.NET MVC

Ez egy teljesen működő ASP.NET MVC alapú autentikációs és felhasználókezelő rendszer, JWT token alapú autentikációval, refresh tokenekkel, regisztrációval, jelszóváltoztatással és visszaállítással.

Főbb funkciók:

Felhasználói regisztráció (/api/auth/register)

Bejelentkezés email/felhasználónév + jelszó kombinációval (/api/auth/login)

JWT Access token és Refresh token alapú autentikáció

Token frissítés (/api/auth/refresh)

Kijelentkezés (/api/auth/logout)

Jelenlegi felhasználó lekérdezése (/api/auth/me)

Jelszó módosítás és visszaállítás (DTO alapú)

Email megerősítés és tokenkezelés

MySQL adatbázis integráció Entity Framework Core segítségével

Swagger/OpenAPI dokumentáció a fejlesztéshez és teszteléshez

📂 Fájlstruktúra
LoginSystem/
│

├─ Controllers/

│   └─ AuthController.cs         # API végpontok kezelése (login, register, refresh, logout)

│

├─ Data/

│   └─ ApplicationDbContext.cs   # EF Core DbContext és tábla konfigurációk

│

├─ Model/

│   └─ User.cs                   # Felhasználói entitás

│

├─ DTOs/

│   ├─ LoginRequest.cs

│   ├─ RegisterRequest.cs

│   ├─ ChangePasswordRequest.cs

│   ├─ ForgotPasswordRequest.cs

│   ├─ ResetPasswordRequest.cs

│   └─ RefreshTokenRequest.cs

│

├─ appsettings.json              # Adatbázis és JWT konfiguráció

├─ Program.cs                    # Alkalmazás indulása, middleware és szolgáltatások regisztrálása



⚙️ Telepítés

Klónozd a repót:

git clone https://github.com/<felhasznalonev>/LoginSystem.git
cd LoginSystem


Nyisd meg Visual Studio-ban.

Ellenőrizd az appsettings.json adatbázis beállításait:

"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=bejelentkezesiRendszer;Uid=root;Pwd=;CharSet=utf8mb4;"
},
"JwtSettings": {
    "SecretKey": "32+ karakteres titkos kulcs",
    "Issuer": "YourAppIssuer",
    "Audience": "YourAppAudience",
    "ExpirationInMinutes": 10,
    "RefreshTokenExpirationInDays": 15
}


Telepítsd a NuGet csomagokat:

dotnet restore


Futtasd az alkalmazást:

dotnet run


Nyisd meg a Swagger UI-t a böngésződben (fejlesztés alatt):

https://localhost:<port>/swagger/index.html

🔐 Használat

Regisztráció: POST /api/auth/register

Bejelentkezés: POST /api/auth/login → Visszaadja az AccessToken + RefreshToken

Token frissítés: POST /api/auth/refresh

Kijelentkezés: POST /api/auth/logout

Jelenlegi felhasználó: GET /api/auth/me (JWT token szükséges)

Tokenek

AccessToken: rövid élettartamú (pl. 10 perc) JWT token

RefreshToken: hosszabb élettartamú (pl. 15 nap) token, új access token generálására

🛠️ Használt technológiák

ASP.NET Core 8.0

Entity Framework Core

MySQL adatbázis

JWT alapú autentikáció

BCrypt.Net jelszóhash-elés

Swagger/OpenAPI dokumentáció

💡 Megjegyzés

A jelenlegi konfiguráció fejlesztéshez optimalizált (CORS: AllowAnyOrigin). Éles környezetben érdemes szigorítani.

A jelszavakat soha nem tároljuk plaintext-ben, mindig hash-elve vannak.
