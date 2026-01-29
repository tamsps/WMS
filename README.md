# PLEASE EXECUTE FOLLOW STEP TO SETUP ENVIROMENT

---

## SETUP DATABASE
Detail in DEPLOYMENT_DATABASE.md
---

## START ALL MICROSERVICE
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
Start WebSite in debug or start without debug by open source code by Visual Studio 2022.
Set WMS.Web as Startup project and Run it

---