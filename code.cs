void Unlock()
{
    Bot.GetComponent<Servomotor>(  "lmotor"  ).Locked = false;
    Bot.GetComponent<Servomotor>(  "rmotor"  ).Locked = false;
    Bot.GetComponent<Servomotor>(  "blmotor"  ).Locked = false; 
    Bot.GetComponent<Servomotor>(  "brmotor"  ).Locked = false;
}

void Up(double vel)
{
    Bot.GetComponent<Servomotor>(  "lmotor"  ).Apply( vel, vel );
    Bot.GetComponent<Servomotor>(  "rmotor"  ).Apply( vel, vel );
    Bot.GetComponent<Servomotor>(  "blmotor"  ).Apply( vel, vel );
    Bot.GetComponent<Servomotor>(  "brmotor"  ).Apply( vel, vel );
}

void Right(double rvel)
{
    Bot.GetComponent<Servomotor>(  "lmotor"  ).Apply( rvel, rvel );
    Bot.GetComponent<Servomotor>(  "rmotor"  ).Apply( rvel, rvel - 2*rvel );
    Bot.GetComponent<Servomotor>(  "blmotor"  ).Apply( rvel, rvel );
    Bot.GetComponent<Servomotor>(  "brmotor"  ).Apply( rvel, rvel - 2*rvel );
}

void Left(double rvel)
{
    Right(-rvel);
}

bool DetectCurve(string sensor)
{
    return ( Bot.GetComponent<ColorSensor>( sensor ).Analog ).ToString() == "Preto" && ( Bot.GetComponent<ColorSensor>( "cc" ).Analog ).ToString() != "Preto";
}

bool DetectGreen(string sensor)
{
    return ( ( Bot.GetComponent<ColorSensor>( sensor ).Analog ).Green ) > ( ( Bot.GetComponent<ColorSensor>( sensor ).Analog ).Blue ) + 10 && ( ( Bot.GetComponent<ColorSensor>( sensor ).Analog ).Green ) > ( ( Bot.GetComponent<ColorSensor>( sensor ).Analog ).Red ) + 10;
}

string GetColorSensor(string sensor)
{
    return ( Bot.GetComponent<ColorSensor>( sensor ).Analog ).ToString();
}

double GetUltraSensor(string sensor)
{
    return ( Bot.GetComponent<UltrasonicSensor>( sensor ).Analog );
}

async Task AlignItself(double rvel)
{
    while (GetColorSensor("cc") != "Preto")
    {
        await Time.Delay(50);
        Right(rvel);
    }
   
}

bool Between(double val, double range1, double range2)
{
     return val >= range1 && val <= range2;
}


bool VerifySensorColorExit(string name)
{
    double red = ( ( Bot.GetComponent<ColorSensor>( name ).Analog ).Red );
    double green = ( ( Bot.GetComponent<ColorSensor>( name ).Analog ).Green );
    double blue = ( ( Bot.GetComponent<ColorSensor>( name ).Analog ).Blue );

    return Between(red, 60, 80) && Between(green, 70, 90) && Between(blue, 80, 100);
}

bool IsExit()
{
    return VerifySensorColorExit("rc1") && VerifySensorColorExit("lc1");
}

bool VerifyUltra(string name, double lim, bool min = true) {

    if(min) {
        return ( Bot.GetComponent<UltrasonicSensor>( name ).Analog ) < lim && ( Bot.GetComponent<UltrasonicSensor>( name ).Analog ) > 0;
    } else {
        return ( Bot.GetComponent<UltrasonicSensor>( name ).Analog ) > lim && ( Bot.GetComponent<UltrasonicSensor>( name ).Analog ) != -1;
    }

}

bool DetectRightAngle()
{
    if (DetectCurve("rc1") && DetectCurve("rc2") && !DetectCurve("lc1"))
    {
        return true;
    }
    return false;
}
bool DetectLeftAngle()
{
    if (DetectCurve("lc1") && DetectCurve("rc2") && !DetectCurve("lc1"))
    {
        return true;
    }
    return false;
}

async Task TurnAngle(double speed, double angle)
{
    double compass = Bot.Compass;
    while( Math.Round(Bot.Compass) != Utils.Modulo(Math.Round(compass+angle), 360) ) {

        await Time.Delay(50);
        if (angle > 0)
        {
            Right(speed);
        }
        else
        {
            Left(speed);
        }

    }
}

async Task Main()
{
    Unlock();
    int vel = 140;
    int rvel = 300;

    bool rc1 = false;
    bool lc1 = false;

    while(true) {
        await Time.Delay(50);
        
        Up(vel);

        if (IsExit())
        {
            break;
        }

        if (GetUltraSensor("ffultra") < 3 && GetUltraSensor("ffultra") != -1)
        {
            Up(vel);
            await Time.Delay(1000);

            if (GetUltraSensor("ffultra") < 3 && GetUltraSensor("ffultra") != -1)
            {
                Up(-vel);
                await Time.Delay(3000);
                await TurnAngle(100, 45);
                Up(vel);
                await Time.Delay(5000);
                await TurnAngle(100, -45);
            }
        }

        if (DetectGreen("rc1") && rc1 && DetectGreen("lc1") && lc1)
        {
            IO.Print("start");
            await TurnAngle(100, 180);
            IO.Print("end");
        }

        if ((( DetectGreen("rc1") || DetectGreen("rc2") )&& ( !DetectGreen("lc1") && !DetectGreen("lc2") )) && rc1 && lc1)
        {
            IO.Print("RC");
            await Time.Delay(100);
            await TurnAngle(100, 70);
            Up(vel);
            await Time.Delay(500);
        }

        if ((( DetectGreen("lc1") || DetectGreen("lc2") ) && ( !DetectGreen("rc1") && !DetectGreen("rc2") )) && lc1 && rc1)
        {
            IO.Print("LC");
            await Time.Delay(100);
            await TurnAngle(100, -70);
            Up(vel);
            await Time.Delay(500);
        }

        while( DetectCurve("rc1") || DetectCurve("rc2")) {
            await Time.Delay(50);
            
            Right(rvel);

        }
        while( DetectCurve("lc1") || DetectCurve("lc2")) {
            await Time.Delay(50);

            Left(rvel);

        }

        rc1 = GetColorSensor("rc1") == "Preto" ? false : true;
        lc1 = GetColorSensor("lc1") == "Preto" ? false : true;

        

    }
    IO.Print("rescue1");
    double t = 0;
    rvel = 100;
    while(true) {
        await Time.Delay(50);
        Up(vel);
        t = GetUltraSensor("ffultra");

        IO.Print(t.ToString());
        if( ( Bot.GetComponent<ColorSensor>( "cc" ).Analog ).ToString() == "Preto") {
            await Time.Delay(1000);
            break;
        }
        else if(Between(GetUltraSensor("ffultra"), 1, 3)) {
            await TurnAngle(rvel, 90);
        }
    }

    int counter = 0;
    IO.Print("rescue2");

    

    while(true) {
        await Time.Delay(50);
        Up(vel);
        if( counter == 3) {
            IO.Print("fim");
            await TurnAngle(rvel, 45);

            break;
        }
        else if(Between(GetUltraSensor("ffultra"), 1, 3)) {
            counter ++;
            await TurnAngle(rvel, 90);
        }
    }
    Up(vel);

}
