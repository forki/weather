#r "./packages/FSharp.Configuration/lib/net40/FSharp.Configuration.dll"
#r "./packages/FSharp.Configuration/lib/net40/SharpYaml.dll"
#r "./packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open System
open FSharp.Configuration
open FSharp.Data

type Config = YamlConfig<"./sample-config.yaml", ReadOnly=true>

type Auth = JsonProvider<"./data/auth.json">

type NetatmoWeather = JsonProvider<"./data/weather.json">

let config = Config()
let configFilePath = "./config.yaml"

config.Load(configFilePath)

let clientId = config.Netatmo.Auth.ClientId
let clientSecret = config.Netatmo.Auth.ClientSecret
let username = config.Netatmo.Auth.Username
let password = config.Netatmo.Auth.Password

let authResponse clientId clientSecret username password =  
        Http.RequestString 
            (config.Netatmo.Contact.BaseUri.ToString() + config.Netatmo.Contact.AuthEndPoint, 
                body = FormValues [
                    ("grant_type", "password");
                    ("client_id", clientId);
                    ("client_secret", clientSecret);
                    ("username", username);
                    ("password", password);
                    ("scope", "read_station")]
            )

let weatherResponse accessToken = 
        Http.RequestString 
            (config.Netatmo.Contact.BaseUri.ToString() + config.Netatmo.Contact.DataEndPoint, 
                body = FormValues [
                    ("access_token", accessToken);
                    ]
            )
let toDateTime (timestamp: int) =
    let start = DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)
    start.AddSeconds(float timestamp)

let toDateTimeLocal (timestamp: int) =
    let start = DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)
    start.AddSeconds(float timestamp).ToLocalTime()
    
let auth = authResponse clientId clientSecret username password
let ar = Auth.Parse(auth)
let accessToken = ar.AccessToken

let weather = weatherResponse accessToken

let w = NetatmoWeather.Parse(weather)     

let b = w.Body
let u = b.User

let d = b.Devices

for dc in d do
    printfn "Indoor Temp: %f" dc.DashboardData.Temperature


for dc in d do
    let m = dc.Modules
    for md in m do
        printfn "%s" md.ModuleName

(*printfn "Device: %s" stationName
        printfn ""
        printfn "Indoor measurement time(UTC): %s" (toDateTime(timeUtc).ToString())
        printfn "Indoor measurement time(Local): %s" (toDateTimeLocal(timeUtc).ToString())
        printfn "Indoor last status store time(UTC): %s" (toDateTime(lastStatusStore).ToString())
        printfn "Indoor last status store time(Local): %s" (toDateTimeLocal(lastStatusStore).ToString())
        printfn "Indoor setup time(UTC): %s" (toDateTime(dateSetup).ToString())
        printfn "Indoor setup time(Local): %s" (toDateTimeLocal(dateSetup).ToString())
        printfn "Indoor last setup time(UTC): %s" (toDateTime(lastSetup).ToString())
        printfn "Indoor last setup time(Local): %s" (toDateTimeLocal(lastSetup).ToString())
        printfn "Indoor last upgrade time(UTC): %s" (toDateTime(lastUpgrade).ToString())
        printfn "Indoor last upgrade time(Local): %s" (toDateTimeLocal(lastUpgrade).ToString())
        printfn "CO2 Calibrating: %b" dc.Co2Calibrating
        printfn "Indoor wifi status: %i" dc.WifiStatus
        printfn "Indoor firmware: %i" dc.Firmware
        printfn "Indoor Temp: %.2f" (ctof (float(dc.DashboardData.Temperature)))
        printfn "Indoor Temp trend: %s" dc.DashboardData.TempTrend
        printfn "Indoor Humidity: %i" dc.DashboardData.Humidity
        printfn "Pressure: %M" dc.DashboardData.Pressure
        printfn "Pressure trend: %s" dc.DashboardData.PressureTrend
        printfn "Absolute Presssure: %M" dc.DashboardData.AbsolutePressure
        printfn "Noise: %i" dc.DashboardData.Noise
        printfn "CO2: %i" dc.DashboardData.Co2
        printfn "Max Indoor Temp: %.2f" (ctof (float(dc.DashboardData.MaxTemp)))
        printfn "DateTime of Max Indoor Temp (UTC): %s" (toDateTime(dc.DashboardData.DateMaxTemp).ToString())
        printfn "DateTime of Max Indoor Temp (Local): %s" (toDateTimeLocal(dc.DashboardData.DateMaxTemp).ToString())
        printfn "Min Indoor Temp: %.2f" (ctof (float(dc.DashboardData.MinTemp)))
        printfn "DateTime of Min Indoor Temp (UTC): %s" (toDateTime(dc.DashboardData.DateMinTemp).ToString())
        printfn "DateTime of Min Indoor Temp (Local): %s" (toDateTimeLocal(dc.DashboardData.DateMinTemp).ToString())
*)
        //for m in dc.Modules do
        //    let mn = m.ModuleName
           (* printfn ""
            printfn "Module Name: %s" mn
            printfn "%s Battery Status: %i" mn m.BatteryPercent
            printfn "%s Battery volt: %i" mn m.BatteryVp
            printfn "%s measurement time(UTC): %s" mn (toDateTime(m.DashboardData.TimeUtc).ToString())
            printfn "%s measurement time(Local): %s" mn (toDateTimeLocal(m.DashboardData.TimeUtc).ToString())
            printfn "%s last message time(UTC): %s" mn (toDateTime(m.LastMessage).ToString())
            printfn "%s last message time(Local): %s" mn (toDateTimeLocal(m.LastMessage).ToString())
            printfn "%s last seen time(UTC): %s" mn (toDateTime(m.LastSeen).ToString())
            printfn "%s last seen time(Local): %s" mn (toDateTimeLocal(m.LastSeen).ToString())
            printfn "%s last setup time(UTC): %s" mn (toDateTime(m.LastSetup).ToString())
            printfn "%s last setup time(Local): %s" mn (toDateTimeLocal(m.LastSetup).ToString())
            printfn "%s rf status: %i" mn m.RfStatus
            printfn "%s firmware: %i" mn m.Firmware
            match mn with
                | "Outdoor" -> 
                    let ot = m.DashboardData.Temperature
                        
                    match ot with
                    | Some t -> 
                        printfn "%s Temp: %.2f" mn (ctof (float(t)))
                    | None -> ()
                    
                    let tt = m.DashboardData.TempTrend
                        
                    match tt with
                    | Some t -> printfn "%s Temp trend: %s" mn t
                    | None -> ()

                    let hy = m.DashboardData.Humidity

                    match hy with
                    | Some h -> printfn "%s Humidity: %i" mn h
                    | None -> ()

                    let dmaxt = m.DashboardData.DateMaxTemp
                    let maxt = m.DashboardData.MaxTemp

                    match maxt with
                    | Some t -> printfn "Max %s Temp: %.2f" mn (ctof (float(t)))
                    | None -> ()

                    match dmaxt with
                    | Some t -> 
                        printfn "DateTime of Max %s Temp (UTC): %s" mn (toDateTime(t).ToString())
                        printfn "DateTime of Max %s Temp (Local): %s" mn (toDateTimeLocal(t).ToString())
                    | None -> ()

                    let dmint = m.DashboardData.DateMinTemp
                    let mint = m.DashboardData.MinTemp

                    match mint with
                    | Some t -> printfn "Min %s Temp: %.2f" mn (ctof (float(t)))
                    | None -> ()

                    match dmint with
                    | Some t -> 
                        printfn "DateTime of Min %s Temp (UTC): %s" mn (toDateTime(t).ToString())
                        printfn "DateTime of Min %s Temp (Local): %s" mn (toDateTimeLocal(t).ToString())
                    | None -> ()

                | "Rain" -> 
                    let rn = m.DashboardData.Rain

                    match rn with
                    | Some r -> printfn "%s since last measurement: %.4f" mn (mmtoin (float(r)))
                    | None -> ()
                        
                    let rn1 = m.DashboardData.SumRain1

                    match rn1 with
                    | Some r -> printfn "%s in last hour: %.4f" mn (mmtoin (float(r)))
                    | None -> ()

                    let rn24 = m.DashboardData.SumRain24

                    match rn24 with
                    | Some r -> printfn "%s in last 24 hours: %.4f" mn (mmtoin (float(r)))
                    | None -> ()

                | _ -> ()
            printfn ""*)