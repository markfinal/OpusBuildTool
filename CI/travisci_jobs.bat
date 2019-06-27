echo "Launching Travis project: BuildAMation"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2FBuildAMation/requests
echo "Launching Travis project: bam-compress"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-compress/requests

rem curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\"}}"  https://api.travis-ci.org/repo/markfinal%%2Fbam-imageformats/requests
rem curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\"}}"  https://api.travis-ci.org/repo/markfinal%%2Fbam-python/requests
echo Finished launching TravisCI jobs
goto :eof
