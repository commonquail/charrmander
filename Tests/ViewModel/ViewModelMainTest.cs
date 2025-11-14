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

            vm.SkillPointsTotal.Core.Should().BePositive();
            vm.SkillPointsTotal.Hot.Should().BePositive();
            vm.SkillPointsTotal.Pof.Should().BePositive();
            vm.SkillPointsTotal.Eod.Should().BePositive();
            vm.SkillPointsLocked.Core.Should().Be(vm.SkillPointsTotal.Core);
            vm.SkillPointsLocked.Hot.Should().Be(vm.SkillPointsTotal.Hot);
            vm.SkillPointsLocked.Pof.Should().Be(vm.SkillPointsTotal.Pof);
            vm.SkillPointsLocked.Eod.Should().Be(vm.SkillPointsTotal.Eod);
            vm.SkillPointsSpendable.Should().Be(0);
            vm.SkillPointsUnlocked.Core.Should().Be(0);
            vm.SkillPointsUnlocked.Hot.Should().Be(0);
            vm.SkillPointsUnlocked.Pof.Should().Be(0);
            vm.SkillPointsUnlocked.Eod.Should().Be(0);
        }

        [Fact]
        public void has_32_areas_participating_in_world_completion()
        {
            //    1 Chanty of Secrets
            // + 25 explorable zones
            // = 26
            // +  6 cities
            // = 32
            var vm = new ViewModelMain();
            vm.AreaReferenceList.Count(a => a.ParticipatesInWorldCompletion).Should().Be(32);
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
            vm.SelectedCharacter!.Name += "foo";

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

            vm.SelectedCharacter!.Name.Should().Be("c2");
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

            var selectedCharacter = vm.SelectedCharacter!;
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

            var selectedCharacter = vm.SelectedCharacter!;
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

            var selectedCharacter = vm.SelectedCharacter!;
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

            var selectedCharacter = vm.SelectedCharacter!;
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

            var selectedCharacter = vm.SelectedCharacter!;
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
            selectedCharacter.EliteSpecializations.Select(es => (es.Name, es.Unlocked)).Should()
                .ContainInOrder(("Reaper", true), ("Scourge", false));

            // -245 because we've unlocked one elite spec but only 5 skill points.
            vm.SkillPointsSpendable.Should().Be(-245);

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
            vm.HasCompletedEoD.Should().Be(CompletionState.NotBegun);

            // "Areas" tab.
            selectedCharacter.AreaByName.Should().ContainKey("Blazeridge Steppes");
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

            vm.SkillPointsLocked.Core.Should().Be(209);
            vm.SkillPointsLocked.Hot.Should().Be(400);
            vm.SkillPointsLocked.Pof.Should().Be(290);
            vm.SkillPointsLocked.Eod.Should().Be(310);
            vm.SkillPointsLocked.Soto.Should().Be(0);
            vm.SkillPointsLocked.Jw.Should().Be(0);
            vm.SkillPointsLocked.Vot.Should().Be(300);

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

            var selectedCharacter = vm.SelectedCharacter!;
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
            vm.AreaReferenceList.Should().NotBeEmpty();
            vm.SelectedAreaReference = vm.AreaReferenceList[0];
            vm.SelectedAreaCharacter.Should().NotBeNull();
            vm.SelectedAreaReference.State.Should().Be(CompletionState.NotBegun);
            vm.CommandCompleteArea.Execute(null);
            vm.SelectedAreaReference.State.Should().Be(CompletionState.Completed);
        }

        [Fact]
        public void completing_area_counts_locked_skill_points()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            vm.AreaReferenceList.Should().NotBeEmpty();
            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "Auric Basin");
            vm.SelectedAreaCharacter.Should().NotBeNull();
            vm.SelectedAreaReference.State.Should().Be(CompletionState.NotBegun);
            vm.SkillPointsUnlocked.Hot.Should().Be(0);
            vm.CommandCompleteArea.Execute(null);
            vm.SelectedAreaReference.State.Should().Be(CompletionState.Completed);
            vm.SkillPointsUnlocked.Hot.Should().Be(110);
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
        public void progress_skills_objective_counts_locked_and_spendable_skill_points()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "Diessa Plateau");
            vm.Skills = "1";

            vm.SkillPointsUnlocked.Core.Should().Be(1);
            vm.SkillPointsLocked.Core.Should().Be(vm.SkillPointsTotal.Core - vm.SkillPointsUnlocked.Core);
            vm.SkillPointsSpendable.Should()
                .Be(vm.SkillPointsUnlocked.Core);

            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "Auric Basin");
            vm.Skills = "2";
            vm.SkillPointsUnlocked.Hot.Should().Be(20);
            vm.SkillPointsLocked.Hot.Should().Be(vm.SkillPointsTotal.Hot - vm.SkillPointsUnlocked.Hot);
            vm.SkillPointsSpendable.Should()
                .Be(vm.SkillPointsUnlocked.Hot + vm.SkillPointsUnlocked.Core);

            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "Crystal Oasis");
            vm.Skills = "3";
            vm.SkillPointsUnlocked.Pof.Should().Be(30);
            vm.SkillPointsLocked.Pof.Should().Be(vm.SkillPointsTotal.Pof - vm.SkillPointsUnlocked.Pof);
            vm.SkillPointsSpendable.Should()
                .Be(vm.SkillPointsUnlocked.Pof + vm.SkillPointsUnlocked.Hot + vm.SkillPointsUnlocked.Core);

            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "The Echovald Wilds");
            vm.Skills = "4";
            vm.SkillPointsUnlocked.Eod.Should().Be(40);
            vm.SkillPointsLocked.Eod.Should().Be(vm.SkillPointsTotal.Eod - vm.SkillPointsUnlocked.Eod);
            vm.SkillPointsSpendable.Should()
                .Be(
                    vm.SkillPointsUnlocked.Eod
                    + vm.SkillPointsUnlocked.Pof
                    + vm.SkillPointsUnlocked.Hot
                    + vm.SkillPointsUnlocked.Core);
        }

        [Fact]
        public void change_character_counts_locked_and_spendable_skill_points()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "Auric Basin");
            vm.Skills = "1";
            vm.SkillPointsUnlocked.Hot.Should().Be(10);
            vm.SkillPointsLocked.Hot.Should().Be(vm.SkillPointsTotal.Hot - vm.SkillPointsUnlocked.Hot);
            vm.SkillPointsSpendable.Should().Be(vm.SkillPointsUnlocked.Hot);

            vm.CommandNewCharacter.Execute(null);
            vm.SelectedCharacter = vm.SortedCharacterList.View.Cast<Character>().Last();

            vm.SkillPointsLocked.Hot.Should().Be(vm.SkillPointsTotal.Hot);
            vm.SkillPointsSpendable.Should().Be(0);

            vm.SelectedAreaReference = vm.AreaReferenceList.First(a => a.Name == "Crystal Oasis");
            vm.Skills = "2";
            vm.SkillPointsUnlocked.Pof.Should().Be(20);
            vm.SkillPointsLocked.Pof.Should().Be(vm.SkillPointsTotal.Pof - vm.SkillPointsUnlocked.Pof);
            vm.SkillPointsSpendable.Should().Be(vm.SkillPointsUnlocked.Pof);

            vm.SelectedCharacter = vm.SortedCharacterList.View.Cast<Character>().First();
            vm.SkillPointsLocked.Pof.Should().Be(vm.SkillPointsTotal.Pof);
            vm.SkillPointsSpendable.Should().Be(vm.SkillPointsUnlocked.Hot);
        }

        [Fact]
        public void change_profession_does_not_reset_unlocked_elite_specializations()
        {
            // This is a convenience that lets users switch professions
            // (deliberately or accidentally) without immediately losing
            // unlocked elite specs.

            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            // 1. Assign some profession.
            // 2. Unlock some elite spec.
            // 3. Switch to another profession.
            // 4. See that no elite spec is unlocked.
            // 5. Unlock some elite spec.
            // 6. Switch back to the first profession.
            // 7. See that its previously unlocked elite spec remains unlocked.

            var selectedCharacter = vm.SelectedCharacter!;
            selectedCharacter.Profession = "Necromancer";

            selectedCharacter.EliteSpecializations[0].Unlocked = true;

            selectedCharacter.Profession = "Mesmer";
            selectedCharacter.EliteSpecializations.Should().NotContain(es => es.Unlocked);
            selectedCharacter.EliteSpecializations[0].Unlocked = true;

            selectedCharacter.Profession = "Necromancer";
            selectedCharacter.EliteSpecializations[0].Unlocked.Should().BeTrue();
        }

        [Fact]
        public void unlock_elite_specialization_counts_spendable_skill_points()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            var selectedCharacter = vm.SelectedCharacter!;
            selectedCharacter.Profession = "Necromancer";
            vm.SkillPointsSpendable.Should().Be(0);

            // Spendable skill points are recalculated actively, not
            // automatically, so this doesn't work.
            selectedCharacter.EliteSpecializations[0].Unlocked = true;
            vm.SkillPointsSpendable.Should().Be(0);

            // This is what works. However, the property we desire is the causal
            // relationship between unlocking elite specializations and
            // spendable skill points.
            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(-250);

            // And now the inverse property.
            selectedCharacter.EliteSpecializations[0].Unlocked = false;
            vm.SkillPointsSpendable.Should().Be(-250);

            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(0);
        }

        [Fact]
        public void change_profession_counts_spendable_skill_points()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);

            // 1. Assign some profession.
            // 2. Unlock some elite spec.
            // 3. See that spendable skill points changes.
            // 4. Assign some other profession.
            // 5. See that spendable skill points changes.
            // 6. Unlock some elite spec.
            // 7. See that spendable skill points changes again.
            // 8. Switch back to the first profession.
            // 9. See that spendable skill points changes back.

            var selectedCharacter = vm.SelectedCharacter!;
            selectedCharacter.Profession = "Necromancer";
            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(0);

            selectedCharacter.EliteSpecializations[0].Unlocked = true;
            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(-250);

            selectedCharacter.Profession = "Mesmer";
            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(0);
            selectedCharacter.EliteSpecializations[0].Unlocked = true;
            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(-250);

            selectedCharacter.Profession = "Necromancer";
            vm.ComputeAvailableSkillPoints();
            vm.SkillPointsSpendable.Should().Be(-250);
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

        [Fact]
        public void sorted_area_list_is_in_ascended_order_by_name()
        {
            var vm = new ViewModelMain();

            var sortedAreas = vm.SortedAreas.Cast<Area>().ToList();

            sortedAreas.Should().BeInAscendingOrder(c => c.Name);
        }

        [Fact]
        public void sorted_area_list_can_filter_by_required_for_world_completion()
        {
            var vm = new ViewModelMain();

            var defaultAreaList = vm.SortedAreas.Cast<Area>().ToList();

            vm.AreaFilter = AreaFilter.WorldCompletion;
            var worldCompletionAreaList = vm.SortedAreas.Cast<Area>().ToList();

            worldCompletionAreaList.Should()
                .NotBeEmpty()
                .And.BeSubsetOf(defaultAreaList)
                .And.HaveCountLessThan(defaultAreaList.Count);

            vm.AreaFilter = AreaFilter.All;
            var allAreaList = vm.SortedAreas.Cast<Area>().ToList();

            allAreaList.Should().BeEquivalentTo(defaultAreaList);
        }

        [Fact]
        public void completing_non_world_completion_area_cannot_affect_world_completion()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            var selectedCharacter = vm.SelectedCharacter!;

            var someNonwWorldCompletionArea = vm.AreaReferenceList.First(a => a.Name == "Auric Basin");
            CompleteArea(vm, someNonwWorldCompletionArea);
            selectedCharacter.HasWorldCompletion.Should().BeFalse();
        }

        [Fact]
        public void completing_single_world_completion_area_does_not_grant_world_completion()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            var selectedCharacter = vm.SelectedCharacter!;

            var someWorldCompletionArea = vm.AreaReferenceList.First(
                a => a.Name == vm.WorldCompletionAreaNames.First());

            CompleteArea(vm, someWorldCompletionArea);
            selectedCharacter.HasWorldCompletion.Should().BeFalse();
        }

        [Fact]
        public void completing_every_world_completion_area_grants_world_completion()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            var selectedCharacter = vm.SelectedCharacter!;

            selectedCharacter.HasWorldCompletion.Should().BeFalse();

            foreach (var a in vm.AreaReferenceList)
            {
                if (vm.WorldCompletionAreaNames.Contains(a.Name))
                    CompleteArea(vm, a);
            }

            selectedCharacter.HasWorldCompletion.Should().BeTrue();
        }

        [Fact]
        public void uncompleting_any_area_does_not_revoke_world_completion()
        {
            var vm = new ViewModelMain();
            vm.CommandNewCharacter.Execute(null);
            var selectedCharacter = vm.SelectedCharacter!;

            foreach (var a in vm.AreaReferenceList)
            {
                CompleteArea(vm, a);
            }

            selectedCharacter.HasWorldCompletion.Should().BeTrue();

            foreach (var a in vm.AreaReferenceList)
            {
                a.PoIs = "0";
            }

            selectedCharacter.HasWorldCompletion.Should().BeTrue();
        }

        private static IEnumerable<Chapter> ChaptersOfAct(Act a) => a.Chapters;

        private static void CompleteArea(ViewModelMain vm, Area a)
        {
            vm.SelectedAreaReference = a;
            vm.CommandCompleteArea.Execute(null);
        }

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
