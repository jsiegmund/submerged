#include <Wire.h>
#include <OneWire.h>
#include <DallasTemperature.h>

#define ONE_WIRE_BUS 3
#define SensorPin 0            //pH meter Analog output to Arduino Analog Input 0
#define Offset 0.00            //deviation compensate

unsigned long int avgValue;     //Store the average value of the sensor feedback

// Setup a oneWire instance to communicate with any OneWire devices (not just Maxim/Dallas temperature ICs)
OneWire oneWire(ONE_WIRE_BUS);

// Pass our oneWire reference to Dallas Temperature. 
DallasTemperature sensors(&oneWire);

// arrays to hold device address
DeviceAddress thermometer1, thermometer2;

float temp1, temp2, ph;

void setup()
{
  pinMode(13,OUTPUT);  
  Serial.begin(9600);  
  Serial.println("Ready");    //Test the serial monitor

  // locate devices on the bus
  Serial.print("Locating devices...");
  sensors.begin();
  Serial.print("Found ");
  Serial.print(sensors.getDeviceCount(), DEC);
  Serial.println(" devices.");

  // report parasite power requirements
  Serial.print("Parasite power is: "); 
  if (sensors.isParasitePowerMode()) Serial.println("ON");
  else Serial.println("OFF");
  
  if (!sensors.getAddress(thermometer1, 0)) Serial.println("Unable to find address for Device 0"); 
  if (!sensors.getAddress(thermometer2, 1)) Serial.println("Unable to find address for Device 1"); 

  // show the addresses we found on the bus
  Serial.print("Device 0 Address: ");
  printAddress(thermometer1);
  Serial.println();

  Serial.print("Device 1 Address: ");
  printAddress(thermometer2);
  Serial.println();

  // set the resolution to 9 bit (Each Dallas/Maxim device is capable of several different resolutions)
  sensors.setResolution(thermometer1, 9);
  sensors.setResolution(thermometer2, 9);
 
  Serial.print("Device 0 Resolution: ");
  Serial.print(sensors.getResolution(thermometer1), DEC); 
  Serial.println();

  Serial.print("Device 1 Resolution: ");
  Serial.print(sensors.getResolution(thermometer2), DEC); 
  Serial.println();

  Serial.println("Beginning communication on slave #18");
  Wire.begin(18);
  Wire.onRequest(requestEvent); // data request to slave
}

// function to print a device address
void printAddress(DeviceAddress deviceAddress)
{
  for (uint8_t i = 0; i < 8; i++)
  {
    // zero pad the address if necessary
    if (deviceAddress[i] < 16) Serial.print("0");
    Serial.print(deviceAddress[i], HEX);
  }
}

void requestEvent() {
  Serial.println("request event");

  byte message[12];
  // write temp1 to the wire
  float2Bytes(temp1, &message[0]);
  float2Bytes(temp2, &message[4]);
  float2Bytes(ph, &message[8]);
  Wire.write(message, 12);

  Serial.println("Sent data to host");
}

void float2Bytes(float val, byte* bytes_array){
  // Create union of shared memory space
  union {
    float float_variable;
    byte temp_array[4];
  } u;
  // Overite bytes of union with float variable
  u.float_variable = val;
  // Assign bytes to input array
  memcpy(bytes_array, u.temp_array, 4);
}

float getPh()
{
  int rawValue = analogRead(SensorPin);
  int buf[10];                //buffer for read analog
  
  for(int i=0;i<10;i++)       //Get 10 sample value from the sensor for smooth the value
  { 
    buf[i]=1024 - analogRead(SensorPin);
    delay(10);
  }
  for(int i=0;i<9;i++)        //sort the analog from small to large
  {
    for(int j=i+1;j<10;j++)
    {
      if(buf[i]>buf[j])
      {
        int temp=buf[i];
        buf[i]=buf[j];
        buf[j]=temp;
      }
    }
  }
  
  avgValue=0;
  for(int i=2;i<8;i++)                      //take the average value of 6 center sample
    avgValue+=buf[i];
    
  float phValue=(float)avgValue*5.0/1024/6; //convert the analog into millivolt
  phValue=3.5*phValue+Offset;                      //convert the millivolt into pH value\

  float adjustedPh = (-0.01796 * phValue + 1.8056) * phValue - 2.94;
  return adjustedPh;
}

void loop()
{
  // get the pH from the sensor board
  ph = getPh();

  // call sensors.requestTemperatures() to issue a global temperature 
  // request to all devices on the bus
  sensors.requestTemperatures(); // Send the command to get temperatures

  temp1 = sensors.getTempC(thermometer1);
  temp2 = sensors.getTempC(thermometer2);
  
  Serial.print("pH: ");  
  Serial.print(ph,2);
  Serial.print("; temp1: ");  
  Serial.print(temp1,2);
  Serial.print("; temp2: ");  
  Serial.print(temp2,2);
  Serial.println(" ");

  digitalWrite(13, HIGH);       
  delay(1900);
  digitalWrite(13, LOW); 
}
