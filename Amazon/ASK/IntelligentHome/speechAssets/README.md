#Sample Utterances

The following are examples of formatting of utterances:

'''
IntentName  this is a sample utterance with no slots
IntentName  this is a sample utterance containing a {SlotName}
IntentName  this is a sample utterance containing a {SlotName} and {AnotherSlotName}
'''

##Rules for Sample Utterances

While contents of any custom slot values can be entered in different forms (digits, abbreviations, etc.), the surrounding sample utterances need to follow certain rules, as follows. These rules also apply to any words specified inline in AMAZON.LITERAL slot values.

*Numbers should be expressed as words and not digits (“five”, not “5”).
*Acronyms and other phrases involving spoken letters should be separated by periods and spaces (“n. b. a.”, not “nba”).
  *Punctuation marks should not be included except in the special cases listed below. (“ten dollars”, not “$10”. “three point five stars”, not “3.5 stars”). Exceptions:
    *Include periods after letter abbreviations (“n. b. a.”, “e. t. a.”).
    *Include apostrophes in possessive and contractions (“romeo’s” and “i’m”).
    *Include hyphens that are word-internal (“man-eating”) but in no other cases.
  *If the word for a slot value may have apostrophes indicating the possessive, or any other similar punctuation (such as periods or hyphens) include those within the brackets defining the slot. Do not add 's after the closing bracket. For example:
    *For possessives
      *Use: "martini's" as a custom value for {Drink} and "tell me a {Drink} ingredients" in the sample utterances
      *Do not use: "tell me a {Drink}'s ingredients"
    *For hyphenated values
	  *Use: editor-in-chief in the custom values and {Position} in the sample utterances
	  *Do not use: editor in the custom values and {Position}-in-chief in the sample utterances