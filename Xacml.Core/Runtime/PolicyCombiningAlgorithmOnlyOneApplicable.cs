/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is www.com code.
 *
 * The Initial Developer of the Original Code is
 * Lagash Systems SA.
 * Portions created by the Initial Developer are Copyright (C) 2004
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *   Diego Gonzalez <diegog@com>
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either of the GNU General Public License Version 2 or later (the "GPL"),
 * or the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

using System;
using ctx = Xacml.Core.Context;
using pol = Xacml.Core.Policy;
using inf = Xacml.Core.Interfaces;

namespace Xacml.Core.Runtime
{
	/// <summary>
	/// The policy combining algorithm described in the Appendix C.4. This class is a 
	/// translation of the pseudo-code placed in the documentation.
	/// </summary>
	public class PolicyCombiningAlgorithmOnlyOneApplicable : inf.IPolicyCombiningAlgorithm
	{
		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public PolicyCombiningAlgorithmOnlyOneApplicable()
		{
		}

		#endregion

		#region Public methods

		/// <summary>
		/// The evaluation implementation in the pseudo-code described in the specification.
		/// </summary>
		/// <param name="context">The evaluation context instance.</param>
		/// <param name="policies">The policies that must be evaluated.</param>
		/// <returns>The final decission for the combination of the policy evaluation.</returns>
		public Decision Evaluate( EvaluationContext context, IMatchEvaluableCollection policies )
		{
			Boolean atLeastOne = false;
			Policy selectedPolicy = null;
			TargetEvaluationValue appResult;
			for( int i = 0; i < policies.Count; i++ )
			{
				Policy tempPolicy = (Policy)policies[ i ];
				appResult = appResult = tempPolicy.Match( context );
				if( appResult == TargetEvaluationValue.Indeterminate )
				{
					context.ProcessingError = true;
					context.TraceContextValues();
					return Decision.Indeterminate;
				}
				if( appResult == TargetEvaluationValue.Match )
				{
					if ( atLeastOne )
					{
						context.ProcessingError = true;
						context.TraceContextValues();
						return Decision.Indeterminate;
					}
					else
					{
						atLeastOne = true;
						selectedPolicy = (Policy)policies[i];
					}
				}
				if ( appResult == TargetEvaluationValue.NoMatch )
				{
					continue;
				}
			}
			if ( atLeastOne )
			{
				Decision retValue = selectedPolicy.Evaluate( context );
				
				context.TraceContextValues();

				if( retValue == Decision.Deny || retValue == Decision.Permit )
				{
					context.ProcessingError = false;
					context.IsMissingAttribute = false;
				}
				return retValue;
			}
			else
			{
				return Decision.NotApplicable;
			}
		}

		#endregion
	}
}
