#include "ACS712.h"

ACS712::ACS712() 
{
	
}

int ACS712::calibrate(ACS712_type type, uint8_t _pin) 
{
  pin = _pin;

	switch (type) 
	{
		case ACS712_05B:
			sensitivity = 0.185;
			break;
		case ACS712_20A:
			sensitivity = 0.100;
			break;
		case ACS712_30A:
			sensitivity = 0.066;
			break;
	  case ACS723:
			sensitivity = 0.400;
			break;
	}

	uint16_t acc = 0;
	for (int i = 0; i < 10; i++) 
	{
		acc += analogRead(pin);
	}
	zero = acc / 10;
	return zero;
}

float ACS712::getCurrentAC(uint16_t frequency) 
{
	uint32_t period = 1000000 / frequency;
	uint32_t t_start = micros();

	uint32_t Isum = 0, measurements_count = 0;
	int32_t Inow;

	while (micros() - t_start < period) 
	{
		Inow = analogRead(pin) - zero;
		Isum += Inow*Inow;
		measurements_count++;
	}

	float Irms = sqrt(Isum / measurements_count) / ADC_SCALE * VREF / sensitivity;
	return Irms;
}