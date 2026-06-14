namespace Pkl_About
{
    /// <summary>
    /// Nodes relating to the package.
    /// </summary>
    public class Pkl_About
    {
        internal Pkl_About() { }

        /// <summary>
        /// Information about the package.
        /// </summary>
        /// <param name="runMe">Set to true to run.</param>
        /// <returns name="gitOpened">If the github was opened.</returns>
        /// <search>About.About.Pickle</search>
        [NodeCategory("Query")]
        public static bool Pickle(bool runMe = false)
        {
            if (runMe)
            {
                var formResult = pklCal.Message("About Pickle",
                    "Thanks for using Pickle!\n\n" +
                    "Pickle is a Dynamo package for Revit that contains a wide variety " +
                    "of nodes, as well as a system for Pickling data to graphs and the canvas.\n\n" +
                    "I hope you enjoy using it!\n\n" +
                    "- Gavin Nicholls, Author of package\n\n" +
                    "Would you like to open the github?", yesNo: true);
                if (formResult.Affirmative)
                {
                    string linkPath = @"https://github.com/aussieBIMguru/Pickles";
                    return new ResourceHelper(linkPath).Open();
                }
            }
            return false;
        }

        /// <summary>
        /// Information about the author's YouTube channel.
        /// </summary>
        /// <param name="runMe">Set to true to run.</param>
        /// <returns name="siteOpened">If the channel was opened.</returns>
        /// <search>About.About.AussieBIMGuru</search>
        [NodeCategory("Query")]
        public static bool AussieBIMGuru(bool runMe = false)
        {
            if (runMe)
            {
                var formResult = pklCal.Message("About Aussie BIM Guru",
                    "Aussie BIM Guru is the author's YouTube channel, focused on " +
                    "educating others in coding and better use of Revit, Rhino and " +
                    "various other AEC applications.\n\n" +
                    "Would you like to go to the channel?", yesNo: true);
                if (formResult.Affirmative)
                {
                    string linkPath = @"https://www.youtube.com/aussiebimguru";
                    return new ResourceHelper(linkPath).Open();
                }
            }
            return false;
        }

        /// <summary>
        /// Information about the author's course/content channel.
        /// </summary>
        /// <param name="runMe">Set to true to run.</param>
        /// <returns name="siteOpened">If the website was opened.</returns>
        /// <search>About.About.BIMGuruEducation</search>
        [NodeCategory("Query")]
        public static bool BIMGuruEducation(bool runMe = false)
        {
            if (runMe)
            {
                var formResult = pklCal.Message("About BIM Guru Education",
                    "BIM Guru Education is the author's course/content platform, where " +
                    "he provides and sells both paid and free Revit content/templates and " +
                    "course on AEC programming.\n\n" +
                    "Would you like to go to the website?", yesNo: true);
                if (formResult.Affirmative)
                {
                    string linkPath = @"https://courses.bimguru.education/";
                    return new ResourceHelper(linkPath).Open();
                }
            }
            return false;
        }
    }
}