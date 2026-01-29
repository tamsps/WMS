# PLEASE EXECUTE FOLLOW STEP TO SETUP ENVIROMENT

---

## SETUP DATABASE
Detail in DEPLOYMENT_DATABASE.md
---

## START ALL MICROSERVICE
1/ Open START_ALL_SERVICES.bat and change PROJECT_ROOT of solution
Simple run file START_ALL_SERVICES.bat
```
NOTE: Make sure that all services must be start without error, if error then system will work did not properly
You also start each service manualy if there any service error during start
1/ Go to root folder of each service
2/ Start CMD
3/ run script: dotnet run
e.g To run Products service manual then 
 F:\PROJECT\STUDY\VMS\WMS.Products.API>dotnet run
```

---
## START WEBSITE
Edit DefaultConnection in WMS.Web and in all services at application.json file (Becuase all service connect to the same db name WMSDB)
Set WMS.Web as Startup project and Run it
Start WMS.Web in debug or start without debug by open source code by Visual Studio 2022.


---