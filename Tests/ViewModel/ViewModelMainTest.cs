using Charrmander.Model;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Xunit;

namespace Charrmander.ViewModel
{
    public class ViewModelMainTest
    {
        public ViewModelMainTest() => RegisterPackUriScheme();

        [Fact]
        public void empty_defaults()
        {
            var vm = new ViewModelMain();

            vm.UnsavedChanges.Should().BeFalse();

            vm.SelectedCharacter.Should().BeNull();
            vm.IsCharacterDetailEnabled.Should().BeFalse();

            vm.IsBiographyVisible.Should().Be(Visibility.Collapsed);
            vm.BiographyOptionsProfession.Should().BeNull();
            vm.BiographyOptionsPersonality.Should().BeNull();
            vm.BiographyOptionsRaceFirst.Should().BeNull();
            vm.BiographyOptionsRaceSecond.Should().BeNull();
            vm.BiographyOptionsRaceThird.Should().BeNull();

            vm.Hearts.Should().BeEmpty();
            vm.Waypoints.Should().BeEmpty();
            vm.PoIs.Should().BeEmpty();
            vm.Skills.Should().BeEmpty();
            vm.Vistas.Should().BeEmpty();
        }

        [Fact]
        public void create_first_character_selects_that_character()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            vm.SelectedCharacter.Should().NotBeNull();
            vm.IsCharacterDetailEnabled.Should().BeTrue();
        }

        [Fact]
        public void create_new_character_raises_unsaved_changes()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            vm.UnsavedChanges.Should().BeTrue();
        }

        [Fact]
        public void change_selected_character_property_raises_unsaved_changes()
        {
            var vm = new ViewModelMain();
            vm.Open("Resources/minimal-one-character.charr");

            vm.UnsavedChanges.Should().BeFalse();
            vm.SelectedCharacter.Name += "foo";

            vm.UnsavedChanges.Should().BeTrue();
        }

        [Fact]
        public void delete_selected_character_with_no_characters_is_safe()
        {
            var vm = new ViewModelMain();
            vm.CommandDeleteCharacter.Execute(null);
        }

        [Fact]
        public void delete_selected_character_with_one_character_selects_null()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            vm.CommandDeleteCharacter.Execute(null);

            vm.SelectedCharacter.Should().BeNull();
        }

        [Fact]
        public void delete_selected_character_with_many_characters_selects_first_by_sort_order()
        {
            var vm = new ViewModelMain();
            vm.Open("Resources/default-sort-order.charr");

            vm.SelectedCharacter.Name.Should().Be("c2");
            vm.CommandDeleteCharacter.Execute(null);
            vm.SelectedCharacter.Name.Should().Be("c4");
        }

        [Fact]
        public void update_biography_options_without_character_is_safe()
        {
            var vm = new ViewModelMain();
            vm.UpdateBiographyOptions();
        }

        [Fact]
        public void update_biography_options_without_profession()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            var selectedCharacter = vm.SelectedCharacter;
            selectedCharacter.Race = "Charr";
            vm.UpdateBiographyOptions();

            vm.IsBiographyVisible.Should().Be(Visibility.Collapsed);
            vm.BiographyOptionsProfession.Should().BeNull();
            vm.BiographyOptionsPersonality.Should().BeNull();
            vm.BiographyOptionsRaceFirst.Should().BeNull();
            vm.BiographyOptionsRaceSecond.Should().BeNull();
            vm.BiographyOptionsRaceThird.Should().BeNull();
        }

        [Fact]
        public void update_biography_options_without_race()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            var selectedCharacter = vm.SelectedCharacter;
            selectedCharacter.Profession = "Warrior";
            vm.UpdateBiographyOptions();

            vm.IsBiographyVisible.Should().Be(Visibility.Collapsed);
            vm.BiographyOptionsProfession.Should().BeNull();
            vm.BiographyOptionsPersonality.Should().BeNull();
            vm.BiographyOptionsRaceFirst.Should().BeNull();
            vm.BiographyOptionsRaceSecond.Should().BeNull();
            vm.BiographyOptionsRaceThird.Should().BeNull();
        }

        [Fact]
        public void update_biography_options_with_race_and_profession_not_ranger()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            var selectedCharacter = vm.SelectedCharacter;
            selectedCharacter.Race = "Charr";
            selectedCharacter.Profession = "Warrior";
            vm.UpdateBiographyOptions();

            vm.IsBiographyVisible.Should().Be(Visibility.Visible);
            vm.BiographyOptionsProfession.Should()
                .Equal("Spangenhelm", "Cap Helm", "No Helm");
            vm.BiographyOptionsPersonality.Should()
                .Equal("Charm", "Dignity", "Ferocity");
            vm.BiographyOptionsRaceFirst.Should()
                .Equal("Blood Legion", "Ash Legion", "Iron Legion");
            vm.BiographyOptionsRaceSecond.Should()
                .Equal(
                    "Maverick",
                    "Euryale",
                    "Clawspur",
                    "Dinky",
                    "Reeva");
            vm.BiographyOptionsRaceThird.Should()
                .Equal(
                    "Loyal Soldier",
                    "Sorcerous Shaman",
                    "Honorless Gladium");
        }

        [Fact]
        public void update_biography_options_with_race_and_profession_ranger()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            var selectedCharacter = vm.SelectedCharacter;
            selectedCharacter.Race = "Sylvari";
            selectedCharacter.Profession = "Ranger";
            vm.UpdateBiographyOptions();

            vm.IsBiographyVisible.Should().Be(Visibility.Visible);
            vm.BiographyOptionsProfession.Should()
                .Equal("Moa", "Stalker", "Fern Hound");
            vm.BiographyOptionsPersonality.Should()
                .Equal("Charm", "Dignity", "Ferocity");
            vm.BiographyOptionsRaceFirst.Should()
                .Equal(
                    "The White Stag",
                    "The Green Knight",
                    "The Shield of the Moon");
            vm.BiographyOptionsRaceSecond.Should()
                .Equal(
                    "Act with wisdom, but act.",
                    "All things have a right to grow.",
                    "Where life goes, so too, should you.");
            vm.BiographyOptionsRaceThird.Should()
                .Equal(
                    "Cycle of Dawn",
                    "Cycle of Noon",
                    "Cycle of Dusk",
                    "Cycle of Night");
        }

        [Fact]
        public void load_single_character()
        {
            var vm = new ViewModelMain();
            var openResult = vm.Open("Resources/minimal-one-character.charr");

            openResult.Should().BeTrue();

            vm.WindowTitle.Should().Be("minimal-one-character.charr - Charrmander");
            vm.CurrentVersion.Should().MatchRegex(@"^\d+.\d+.\d+.\d+$");

            var selectedCharacter = vm.SelectedCharacter;
            selectedCharacter.Should().NotBeNull();

            // "Personal" tab.
            selectedCharacter.Name.Should().Be("minimalish");
            selectedCharacter.Race.Should().Be("Sylvari");
            selectedCharacter.Profession.Should().Be("Necromancer");
            selectedCharacter.Level.Should().Be(80);
            selectedCharacter.BiographyProfession.Should().Be("Ghostly Wraith");
            selectedCharacter.BiographyPersonality.Should().Be("Ferocity");
            selectedCharacter.BiographyRaceFirst.Should().Be("The Shield of the Moon");
            selectedCharacter.BiographyRaceSecond.Should().Be("Where life goes, so too, should you.");
            selectedCharacter.BiographyRaceThird.Should().Be("Cycle of Dusk");

            // "Story" tab.
            selectedCharacter.Order.Should().Be("Order of Whispers");
            selectedCharacter.RacialSympathy.Should().Be("Hylek");
            selectedCharacter.RetributionAlly.Should().Be("OoW: Gorr");
            selectedCharacter.GreatestFear.Should().Be("Dishonored by allies");
            selectedCharacter.PlanOfAttack.Should().Be("Priory: Missing Squad");

            static bool IsKeyRewardChapter(Chapter c) => c.Name.Contains("\U0001F5DD");

            var keyChapterLw2 = selectedCharacter.Lw2Acts
                .Where(a => a.Name == "Tangled Paths")
                .SelectMany(ChaptersOfAct)
                .First(IsKeyRewardChapter);
            keyChapterLw2.ChapterCompleted.Should().BeFalse();
            vm.HasKeyLw2.Should().BeFalse();

            var keyChapterHoT = selectedCharacter.HoTActs
                .Where(a => a.Name == "Act 3")
                .SelectMany(ChaptersOfAct)
                .First(IsKeyRewardChapter);
            keyChapterHoT.ChapterCompleted.Should().BeTrue();
            vm.HasKeyHoT.Should().BeTrue();

            Expression<Func<Chapter, bool>> IsCompletedChapter = c => c.ChapterCompleted;

            selectedCharacter.Lw2Acts
                .SelectMany(ChaptersOfAct)
                .Should().NotContain(IsCompletedChapter);
            vm.HasCompletedLw2.Should().Be(CompletionState.NotBegun);

            selectedCharacter.HoTActs
                .SelectMany(ChaptersOfAct)
                .Should().Contain(IsCompletedChapter);
            vm.HasCompletedHoT.Should().Be(CompletionState.Begun);

            selectedCharacter.KotTActs
                .SelectMany(ChaptersOfAct)
                .Should().OnlyContain(IsCompletedChapter);
            vm.HasCompletedKotT.Should().Be(CompletionState.Completed);

            vm.HasCompletedLw3.Should().Be(CompletionState.Begun);
            vm.HasCompletedPoF.Should().Be(CompletionState.Begun);
            vm.HasCompletedLw4.Should().Be(CompletionState.Begun);
            vm.HasCompletedTis.Should().Be(CompletionState.Begun);

            // "Areas" tab.
            selectedCharacter.Areas.Should().Contain(a => a.Name == "Blazeridge Steppes");
            var someCompletedArea = vm.AreaReferenceList.First(a => a.Name == "Blazeridge Steppes");
            someCompletedArea.State.Should().Be(CompletionState.Completed);
            vm.SelectedAreaCharacter.Should().BeNull();
            vm.SelectedAreaReference = someCompletedArea;
            vm.SelectedAreaCharacter.Should().NotBeNull()
                .And.Subject.As<Area>().Name.Should().Be(someCompletedArea.Name);

            vm.HeartIcon.Should().Be("HeartDone");
            vm.WaypointIcon.Should().Be("WaypointDone");
            vm.PoIIcon.Should().Be("PoIDone");
            vm.SkillIcon.Should().Be("SkillDone");
            vm.VistaIcon.Should().Be("VistaDone");

            vm.Hearts.Should().Be(vm.SelectedAreaReference.Hearts);
            vm.Waypoints.Should().Be(vm.SelectedAreaReference.Waypoints);
            vm.PoIs.Should().Be(vm.SelectedAreaReference.PoIs);
            vm.Skills.Should().Be(vm.SelectedAreaReference.Skills);
            vm.Vistas.Should().Be(vm.SelectedAreaReference.Vistas);

            selectedCharacter.HasWorldCompletion.Should().BeTrue();

            // "Crafting" tab.
            var craftingDisc = selectedCharacter.CraftingDisciplines
                .ToDictionary(cd => cd.Name);
            craftingDisc.Should()
                .HaveSameCount(selectedCharacter.CraftingDisciplines)
                .And.ContainKeys(
                    "Armorsmith",
                    "Artificer",
                    "Chef",
                    "Huntsman",
                    "Jeweler",
                    "Leatherworker",
                    "Scribe",
                    "Weaponsmith");
            craftingDisc["Jeweler"].Level.Should().Be(400);
            craftingDisc["Armorsmith"].Level.Should().Be(0);
            craftingDisc["Artificer"].Level.Should().Be(0);
            craftingDisc["Chef"].Level.Should().Be(500);
            craftingDisc["Huntsman"].Level.Should().Be(0);

            // "Dungeons" tab.
            selectedCharacter.FractalTier.Should().Be(100);
            var dungeonNameByStoryCompleted = selectedCharacter.Dungeons
                .GroupBy(d => d.StoryCompleted, d => d.Name)
                .ToDictionary(g => g.Key, g => g.ToList());
            dungeonNameByStoryCompleted.Should().ContainKey(true);
            dungeonNameByStoryCompleted[true].Should()
                .ContainSingle("Ascalonian Catacombs");
            dungeonNameByStoryCompleted[false].Should()
                .ContainInOrder(
                    "Caudecus's Manor",
                    "Twilight Arbor",
                    "Sorrow's Embrace",
                    "Citadel of Flame",
                    "Honor of the Waves",
                    "Crucible of Eternity",
                    "The Ruined City of Arah");

            // "Notes" tab.
            selectedCharacter.Notes.Should().Be("hello,\nworld!");
        }

        [Fact]
        public void can_save_then_load_same_file()
        {
            var tp = Path.GetTempPath();
            var saveFileName = tp + Guid.NewGuid() + ".charr";

            try
            {
                File.Copy("Resources/two-characters.charr", saveFileName);

                var vm = new ViewModelMain();
                vm.Open(saveFileName);
                vm.CommandSave.Execute(null);

                // Work around blocking UI on "save as" + "reopen same file".
                vm.CommandExit.Execute(null);
                vm.Dispose();
                vm = new ViewModelMain();

                vm.Open(saveFileName);
            }
            finally
            {
                File.Delete(saveFileName);
            }
        }

        [Fact]
        public void can_load_and_migrate_data()
        {
            var vm = new ViewModelMain();
            vm.Open("Resources/data-migration.charr");

            var selectedCharacter = vm.SelectedCharacter;
            selectedCharacter.Should().NotBeNull();

            selectedCharacter.TisActs
                .Should().NotContain(a => a.Name.Equals("calm in the storm", StringComparison.OrdinalIgnoreCase));

            var lw4Chapters = selectedCharacter.Lw4Acts
                .SelectMany(ChaptersOfAct);
            lw4Chapters
                .Should().Contain(a => a.Name == "Scion & Champion")
                .And.NotContain(a => a.Name.EndsWith('>'));
            lw4Chapters
                .First(a => a.Name == "Scion & Champion")
                .ChapterCompleted.Should().BeTrue();
        }

        [Fact]
        public void open_nonexistent_file_returns_false()
        {
            var vm = new ViewModelMain();
            var result = vm.Open(new Guid().ToString());
            result.Should().BeFalse();
        }

        [Fact]
        public void completing_area_sets_selected_area_reference_state_completed()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            vm.SelectedAreaReference = vm.AreaReferenceList.First();
            vm.SelectedAreaCharacter.Should().NotBeNull();
            vm.SelectedAreaReference.State.Should().Be(CompletionState.NotBegun);
            vm.CommandCompleteArea.Execute(null);
            vm.SelectedAreaReference.State.Should().Be(CompletionState.Completed);
        }

        [Fact]
        public void completion_objective_icons_reflect_completion_objective_progress()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            static bool HasAllNonZeroObjectives(Area a) => a.Name == "Queensdale";
            vm.SelectedAreaReference = vm.AreaReferenceList.First(HasAllNonZeroObjectives);

            using (var monitor = vm.Monitor())
            {
                vm.CommandCompleteArea.Execute(null);
                monitor.Should().RaisePropertyChangeFor(x => x.HeartIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Hearts);
                monitor.Should().RaisePropertyChangeFor(x => x.WaypointIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Waypoints);
                monitor.Should().RaisePropertyChangeFor(x => x.PoIIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.PoIs);
                monitor.Should().RaisePropertyChangeFor(x => x.SkillIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Skills);
                monitor.Should().RaisePropertyChangeFor(x => x.VistaIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Vistas);
            }

            vm.HeartIcon.Should().Be("HeartDone");
            vm.WaypointIcon.Should().Be("WaypointDone");
            vm.PoIIcon.Should().Be("PoIDone");
            vm.SkillIcon.Should().Be("SkillDone");
            vm.VistaIcon.Should().Be("VistaDone");

            using (var monitor = vm.Monitor())
            {
                vm.Hearts = "0";
                vm.Waypoints = "0";
                vm.PoIs = "0";
                vm.Vistas = "0";
                vm.Skills = "0";
                monitor.Should().RaisePropertyChangeFor(x => x.HeartIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Hearts);
                monitor.Should().RaisePropertyChangeFor(x => x.WaypointIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Waypoints);
                monitor.Should().RaisePropertyChangeFor(x => x.PoIIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.PoIs);
                monitor.Should().RaisePropertyChangeFor(x => x.SkillIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Skills);
                monitor.Should().RaisePropertyChangeFor(x => x.VistaIcon);
                monitor.Should().RaisePropertyChangeFor(x => x.Vistas);
            }

            vm.HeartIcon.Should().Be("Heart");
            vm.WaypointIcon.Should().Be("Waypoint");
            vm.PoIIcon.Should().Be("PoI");
            vm.SkillIcon.Should().Be("Skill");
            vm.VistaIcon.Should().Be("Vista");
        }

        [Fact]
        public void sorted_character_list_is_in_ascended_order_by_defaultsortorder()
        {
            var vm = new ViewModelMain();
            vm.Open("Resources/default-sort-order.charr");
            var sortedCharacters = vm.SortedCharacterList.View.Cast<Character>();
            sortedCharacters.Should()
                .BeInAscendingOrder(c => c.DefaultSortOrder);
            sortedCharacters.Should()
                .NotBeInAscendingOrder(c => c.Name, "sanity: name, sort order have different ordering");
        }

        private static IEnumerable<Chapter> ChaptersOfAct(Act a) => a.Chapters;

        private static void RegisterPackUriScheme()
        {
            // https://stackoverflow.com/a/6005606/482758
            if (!UriParser.IsKnownScheme("pack"))
            {
                var _ = System.IO.Packaging.PackUriHelper.UriSchemePack;
            }
        }
    }
}
