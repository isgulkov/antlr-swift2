using System;
using System.Collections.Generic;

namespace SwiftTranslator
{
	public class SwiftStatementPlacementCheckListener : SwiftBaseListener
	{
		/// <summary>
		/// Номер строки, на которой окончился предыдущий оператор. Если предыдущий оператор был отделен точкой с
		/// запятой, то поле принимает значение <code>-1</code>, что означает, что следующий за ним оператор может
		/// находиться на той же строке
		/// </summary>
		int PreviousStatementEnd = -1;

		bool? Valid = null;
		List<Tuple<int, int>> _CollisionLocations = new List<Tuple<int, int>>();

		public bool IsValid
		{
			get
			{
				if(Valid.HasValue) {
					return Valid.Value;
				}
				else {
					throw new InvalidProgramException("The tree hasn't been validated yet");
				}
			}
		}

		public IEnumerable<Tuple<int, int>> CollisionLocations
		{
			get
			{
				if(!IsValid) {
					return _CollisionLocations;
				}
				else {
					throw new InvalidOperationException("Can't get collision location " +
					                                    "— the tree has been validated as valid");
				}
			}
		}

		public override void EnterStatement(SwiftParser.StatementContext context)
		{
			/*
			 * Проверить, не находится ли текущий оператор в той же строке, что и предыдущий
			 * 
			 * Если предыдущий оператор был отделен точкой с запятой, то поле `PreviousStatementEnd` приняло значение
			 * `-1`, и текущий оператор может находиться в этой же строке без ошибки
			 */
			if(context.Start.Line == PreviousStatementEnd) {
				Valid = false;

				_CollisionLocations.Add(new Tuple<int, int>(context.Start.Line, context.Start.Column));
			}

			if(context.classDeclStmt() != null || context.loopStmt() != null) {
				/*
				 * Первый член класса и первый оператор цикла может находиться на той же строке, напр.:
				 * 
				 * repeat { print(x) ...
				 */
				PreviousStatementEnd = -1;
			}
		}

		public override void ExitStatement(SwiftParser.StatementContext context)
		{
			if(context.children[context.ChildCount - 1].GetText() != ";") {
				/*
				 * Если данный оператор не отделен точкой с запятой, запомним строку его окончания, чтобы затем
				 * проверить, не начинается ли на этой строке другой, следующий оператор
				 */
				PreviousStatementEnd = context.Stop.Line;
			}
			else {
				/*
				 * Если оператор отделен точкой с запятой, то наличие следующего оператора в той же строке не приведет
				 * к ошибке
				 */
				PreviousStatementEnd = -1;
			}
		}

		public override void ExitFile(SwiftParser.FileContext context)
		{
			if(!Valid.HasValue) {
				Valid = true;
			}
		}
	}
}
