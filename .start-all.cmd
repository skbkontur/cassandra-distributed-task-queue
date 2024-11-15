wsl -u root sh -c "service docker status || (service docker start && echo 'artificially waiting 20s for docker to warmup...' && sleep 20s)"
wsl docker-compose -f docker-compose.yaml up -d --build