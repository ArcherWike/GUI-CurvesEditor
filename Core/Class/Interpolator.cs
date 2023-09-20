using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Curves_editor.Core.Class
{
    static internal class Interpolator
    {
        public static double Linear( double src, double dst, double t )
	    {
		    return src * ( 1.0 - t ) + dst * t;
        }

        public static double QuadraticBezier( double p0, double c0, double p1, double t )
        {
            return c0 + ( 1.0 - t ) * ( 1.0 - t ) * ( p0 - c0 ) + t * t * ( p1 - c0 );
        }

        public static double CubicBezier( double p0, double c0, double c1, double p1, double t )
        {
            double tSquared = t * t;
            double oneMinusTSquared = ( 1.0 - t ) * ( 1.0 - t );

            return oneMinusTSquared * ( 1.0 - t ) * p0 +
                    3.0 * oneMinusTSquared * t * c0 +
                    3.0 * ( 1.0 - t ) * tSquared * c1 +
                    tSquared * t * p1;
        }
    }
}
