$build_configuration = "Release"
$build_runtime = "linux-x64"
$build_framework = "net7.0"
$version = "0.0.4"
$docker_imageName = "nzbhydra-scheduler"

Set-Location -Path $PSScriptRoot
dotnet restore --runtime $build_runtime

dotnet publish "./nzbhydra-schedule.sln" --configuration $build_configuration --runtime $build_runtime --framework $build_framework --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:Version=$version -p:PackageVersion=$version --output "./publish/$build_runtime"

Remove-Item -Path "./docker/publish/" -Force -Recurse -ErrorAction SilentlyContinue
New-Item -Path "./docker/publish" -ItemType Directory
Copy-Item -Path "./publish/$build_runtime/*" -Destination "./docker/publish"

Set-Location -Path ./docker
docker build -t $docker_imageName .