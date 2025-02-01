aiur() { arg="$( cut -d ' ' -f 2- <<< "$@" )" && curl -sL https://gitlab.aiursoft.cn/aiursoft/aiurscript/-/raw/master/$1.sh | sudo bash -s $arg; }

app_name="gptgateway"
repo_path="https://gitlab.aiursoft.cn/aiursoft/gptgateway"
proj_path="src/Aiursoft.GptGateway/Aiursoft.GptGateway.csproj"
fted_path="src/Aiursoft.GptGateway.Frontend"

get_dll_name()
{
    filename=$(basename -- "$proj_path")
    project_name="${filename/.csproj/}"
    dll_name="$project_name.dll"
    echo $dll_name
}

install()
{
    port=$1
    if [ -z "$port" ]; then
        port=$(aiur network/get_port)
    fi
    echo "Installing $app_name... to port $port"

    # Install prerequisites    
    aiur install/dotnet
    aiur install/node

    # Clone the repo
    aiur git/clone_to $repo_path /tmp/repo

    # front end
    fepath="/tmp/repo/$fted_path"
    if [ -d "$fepath" ]; then
        echo "Found frontend folder $fepath, will install node modules."
        npm install --prefix "$fepath" -force --loglevel verbose
        npm run build --prefix "$fepath"
    fi

    # Copy built
    wwwrootPath=$(dirname "/tmp/repo/$proj_path")/wwwroot
    mkdir -p $wwwrootPath
    cp -rv $fepath/dist/* $wwwrootPath

    # Publish the app
    aiur dotnet/publish "/tmp/repo/$proj_path" "/opt/apps/$app_name"
    
    # Register the service
    dll_name=$(get_dll_name)
    aiur services/register_aspnet_service $app_name $port "/opt/apps/$app_name" $dll_name

    # Clean up
    echo "Install $app_name finished! Please open http://$(hostname):$port to try!"
    settings_file_path="/opt/apps/$app_name/appsettings.Production.json"
    echo "Please change the settings in $settings_file_path ASAP to fit your own needs!!!"
    echo "Currently settings may save files to /tmp folder."
    rm /tmp/repo -rf
}

# This will install this app under /opt/apps and register a new service with systemd.
# Example: install 5000
install "$@"
