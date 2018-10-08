https://fsprojects.github.io/Paket/paket-install.html
".paket/paket.exe" install --clean-redirects --redirects --force
".paket/paket.exe" convert-from-nuget
dotnet nuget locals all --clear