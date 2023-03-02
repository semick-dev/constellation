dotnet pack --output=.install -c Release ./drone/downloader/Constellation.Drone.Downloader.csproj
dotnet tool update -g --prerelease --add-source ./.install Constellation.Drone.Downloader

