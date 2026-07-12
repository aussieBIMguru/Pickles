namespace Pickles.Helpers
{
    /// <summary>
    /// A class for handling Family loading behavior.
    /// </summary>
    internal class FamilyLoadOptions : DB.IFamilyLoadOptions
    {
        /// <summary>
        /// If we want to overwrite parameter values.
        /// </summary>
        private readonly bool _overwriteValues;

        /// <summary>
        /// If we want to overwrite nested families.
        /// </summary>
        private readonly bool _overwriteNested;

        /// <summary>
        /// Construct a FamilyLoadOptions object.
        /// </summary>
        /// <param name="overwriteValues">Overwrite parameter values.</param>
        /// <param name="overwriteNested">Overwrite shared, nested families.</param>
        internal FamilyLoadOptions(bool overwriteValues = true, bool overwriteNested = false)
        {
            _overwriteValues = overwriteValues;
            _overwriteNested = overwriteNested;
        }

        /// <summary>
        /// Handle what to do if family already exists.
        /// </summary>
        /// <param name="familyInUse">If the family is in use.</param>
        /// <param name="overwriteParameterValues">If parameters will be overwritten.</param>
        /// <returns>A boolean.</returns>
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = _overwriteValues;
            return true;
        }

        /// <summary>
        /// Handle what to do if a shared, nested family exists.
        /// </summary>
        /// <param name="sharedFamily">The nested family.</param>
        /// <param name="familyInUse">If the family is in use.</param>
        /// <param name="source">The FamilySource to use.</param>
        /// <param name="overwriteParameterValues">If parameters will be overwritten.</param>
        /// <returns>A boolean.</returns>
        public bool OnSharedFamilyFound(DB.Family sharedFamily, bool familyInUse,
            out DB.FamilySource source, out bool overwriteParameterValues)
        {
            source = _overwriteNested ? DB.FamilySource.Family : DB.FamilySource.Project;
            overwriteParameterValues = _overwriteValues;
            return true;
        }
    }
}
