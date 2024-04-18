using System;

public static class Triangle
{

	public static double perimeter(double a, double b, double c)
	{
		return a + b + c;
	}


	public static double area(double a, double b, double c)
	{
		double s = perimeter(a, b, c) / 2;
		return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
	}

	public static void SAS(double a, double c, double beta, ref double alpha, ref double gama, ref double b)
	{
		b = Math.Sqrt((Math.Pow(a, 2)) + Math.Pow(c, 2) - (2 * a * c * Math.Cos(beta)));
		alpha = Math.Asin((a * Math.Sin(beta)) / Math.Sqrt((Math.Pow(a, 2)) + Math.Pow(c, 2) - (2 * a * c * Math.Cos(beta))));
		gama = Math.Asin((c * Math.Sin(beta)) / Math.Sqrt((Math.Pow(a, 2)) + Math.Pow(c, 2) - (2 * a * c * Math.Cos(beta))));

	}

	public static void AAS(double a, double alpha, double beta, ref double gama, ref double b, ref double c)
	{
		gama = Math.PI - (alpha + beta);
		b = a * Math.Sin(beta) / Math.Sin(alpha);
		c = a * Math.Sin(beta) * (1 / Math.Tan(alpha) + 1 / Math.Tan(beta));
	}

	public static void ASA(double alpha, double c, double beta, ref double gama, ref double a, ref double b)
	{
		gama = Math.PI - (alpha + beta);
		a = c * Math.Sin(alpha) / Math.Sin(gama);
		c = c * Math.Sin(beta) / Math.Sin(gama);
	}

	public static void SSS(double a, double b, double c, ref double alpha, ref double beta, ref double gama)
	{
		alpha = Math.Acos((Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2)) / (2 * b * c));
		beta = Math.Acos((Math.Pow(a, 2) + Math.Pow(c, 2) - Math.Pow(b, 2)) / (2 * a * c));
		gama = Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b));
	}

	public static void ASS(double alpha, double a, double c, ref double beta, ref double betaL, ref double gama, ref double b, ref double bLarge)
	{
		double dummy = 0, dummy1=0;
		double tmp = Math.Pow(a, 2) - (Math.Pow(c, 2) * Math.Pow(Math.Sin(alpha), 2));
		if (tmp > 0) {
			b = c * Math.Cos(alpha) - Math.Sqrt(tmp);
			bLarge = c * Math.Cos(alpha) + Math.Sqrt(tmp);
			SSS(a, b, c, ref dummy, ref beta, ref gama);
			SSS(a, bLarge, c, ref dummy, ref dummy1, ref betaL);
		}
	}

	public static double circumcircleRadius(double a, double b, double c)
	{
		return (a * b * c) / Math.Sqrt((a + b + c) * (-a + b + c) * (a - b + c) * (a + b - c));
	}

	public static double inscribedCircleRadius(double a, double b, double c)
	{
		double k = (a + b + c) / 2;
		return Math.Sqrt((k - a) * (k - b) * (k - c) / k);
	}

}
