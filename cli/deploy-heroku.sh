#!/bin/bash

# use the unofficial bash strict mode. This causes bash to behave in a way that makes many classes of subtle bugs impossible.
# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail


usage()
{
    cat <<END
deploy.sh: deploys application to Heroku
Parameters:
-environment
    E.g. DevelopmentHeroku, ProductionHeroku
-h | --help
    Displays this help text and exits the script
END
}

# local variables and constants
container_registry="registry.heroku.com"

# should be passed as argument
environment=""
# build_id="" # Azure DevOps buildId
heroku_app_name_receiver=""
container_registry_user=""
container_registry_password=""


# read command line arguments
while [[ $# -gt 0 ]]; do
  case "$1" in
    --environment )
        environment="$2"; shift 2 ;;
    --heroku-app-name-receiver )
        heroku_app_name_receiver="$2"; shift 2 ;;
    --container-registry-user )
        container_registry_user="$2"; shift 2 ;;
    --container-registry-password )
        container_registry_password="$2"; shift 2 ;;
    -h | --help )
        usage; exit 1 ;;
    *)
        echo "Unknown option $1"
        usage; exit 2 ;;
  esac
done


# CLI input validations
if [[ ! $environment ]]; then
    echo "'environment' must be specified"
    echo ''
    usage
    exit 3
fi
if [[ ! $heroku_app_name_receiver ]]; then
    echo "'heroku-app-name-api' must be specified"
    echo ''
    usage
    exit 3
fi


echo "#################### Environment info ####################"
echo 'Checking prerequisites...'

echo "dotnet version:"
dotnet --version

echo "nvm version:"
if [[ "$OSTYPE" == "linux-gnu" ]]; then
    echo "Running on Linux."
     ~/.nvm/nvm.sh # make nvm command available in the script (linux)
     nvm --version
elif [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    # cygwin: POSIX compatibility layer and Linux environment emulation for Windows
    # msys: Lightweight shell and GNU utilities compiled for Windows (part of MinGW)
    # win32: not sure this can happen.
    echo "Running on Windows."
    nvm version
else
    echo "Running on '$OSTYPE' which is not supported."
    exit 1;
fi

echo "Node version:"
node -v

printf "\n"
printf "\n"


echo "#################### Instaling Heroku CLI... ####################"

if [[ "$OSTYPE" == "linux-gnu" ]]; then
    echo "Running on Linux."
    curl https://cli-assets.heroku.com/install.sh | sh
elif [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    # cygwin: POSIX compatibility layer and Linux environment emulation for Windows
    # msys: Lightweight shell and GNU utilities compiled for Windows (part of MinGW)
    # win32: not sure this can happen.
    echo "Running on Windows."
    echo "Do not install Heroku CLI as it supposed that script is being run on local dev machine."
    echo "Check Heroku CLI version (check installed)."
    #heroku --version
    echo "$(heroku --version)"
else
    echo "Running on '$OSTYPE' which is not supported."
    exit 1;
fi

printf "\n"
printf "\n"


echo "#################### Building and pushing Docker images... ####################"
docker login --username=$container_registry_user --password=$container_registry_password $container_registry

# service name list
services=(
    $heroku_app_name_receiver
)

# service Dockerfile path list
# NB: must match 'services'
serviceDockerfilePathList=(
    MessageReceiver/Dockerfile
)

cd ./UdpMessages
service=""
for i in "${!services[@]}"
do
    service=${services[i]}
    dockerfile=${serviceDockerfilePathList[i]}
    echo "Building image for service '$service'..."
    # for Heroku image should be: registry.heroku.com/<app-name>/web
    # NB: docker build context must be in the directory where .dockerignore located so it can be applied
    docker build --file $dockerfile --tag $container_registry/$service/web .

    # Remove itermediate images to save disk space
    echo "Cleaning up..."

    ## Option 1: list dangling images and pass to remove command (2 commands)
    # docker rmi --force $(docker images --quiet --filter "dangling=true")

    ## Options 2: remove dangling images (1 command)
    # NB: --all removes all unused images that aren't used in active containers (including images that you've built earlier)
    docker image prune --force
done
cd ..
printf "\n"
printf "\n"


echo "#################### Releasing... ####################"

# push image
for i in "${!services[@]}"
do
    service=${services[i]}
    
    echo "Pushing image for service '$service'..."
    docker push $container_registry/$service/web
done
printf "\n"
printf "\n"

# create _netrc which used for Heroku CLI login
netrc_file_path=""
if [[ "$OSTYPE" == "linux-gnu" ]]; then
    echo "Running on Linux."
    netrc_file_path="~/.netrc"
elif [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    netrc_file_path="$HOME/_netrc"
fi
echo "Create Heroku .netrc file."
prev_content=$(cat $netrc_file_path)
next_content="
machine api.heroku.com
    login $container_registry_user
    password $container_registry_password
machine git.heroku.com
    login $container_registry_user
    password $container_registry_password
"
#echo $content >> $netrc_file_path
cat >> $netrc_file_path << EOL
$next_content
EOL

for i in "${!services[@]}"
do
    service=${services[i]}
    dockerfile=${serviceDockerfilePathList[i]}
    echo "Heroku release '$service'..."
    heroku container:release web -a $service
done

# restore prev content
cat > $netrc_file_path << EOL 
$prev_content
EOL

printf "\n"
printf "\n"

echo "#################### Finished ####################"
