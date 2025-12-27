using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using CmlLib.Core.ProcessBuilder;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SaturnLauncher
{
    public class LauncherLogic
    {
        public readonly string ZipUrl = "https://github.com/d4shyb0i/SaturnClient/raw/main/client.zip";
        public readonly string SaturnPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".SaturnLauncher");
        public MSession Session;

        public LauncherLogic() { if (!Directory.Exists(SaturnPath)) Directory.CreateDirectory(SaturnPath); }

        public MinecraftLauncher GetLauncher() => new MinecraftLauncher(new MinecraftPath(SaturnPath));

        public async Task<MSession> TryAutoLogin()
        {
            try
            {
                var loginHandler = new JELoginHandlerBuilder().Build();
                Session = await loginHandler.Authenticate();
                return Session;
            }
            catch { return null; }
        }

        public async Task<MSession> Login()
        {
            var loginHandler = new JELoginHandlerBuilder().Build();
            loginHandler.AccountManager.ClearAccounts();
            Session = await loginHandler.Authenticate();
            return Session;
        }

        public void Logout()
        {
            Session = null;
            string path = Path.Combine(SaturnPath, "microsoft_accounts.json");
            if (File.Exists(path)) File.Delete(path);
        }

        public async Task LaunchGame(Action<string> status)
        {
            if (Session == null) throw new Exception("Login Required");

            string versionPath = Path.Combine(SaturnPath, "versions", "Saturn_Client");

            // If missing, download it first
            if (!Directory.Exists(versionPath))
            {
                status("Downloading Assets...");
                using (var c = new HttpClient())
                {
                    var data = await c.GetByteArrayAsync(ZipUrl);
                    string zipFile = Path.Combine(SaturnPath, "temp.zip");
                    File.WriteAllBytes(zipFile, data);
                    ZipFile.ExtractToDirectory(zipFile, SaturnPath, true);
                    File.Delete(zipFile);
                }
            }

            // This part now runs automatically after the download or if files exist
            status("Launching...");
            var launcher = GetLauncher();

            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 4096,
                Session = Session,
                VersionType = "Saturn Client"
            };

            // Instant launch logic
            var process = await launcher.CreateProcessAsync("Saturn_Client", launchOption);
            process.Start();
        }
    }
}