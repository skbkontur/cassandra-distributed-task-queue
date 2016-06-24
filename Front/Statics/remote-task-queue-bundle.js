import {registerClickAction} from '../../../WebWorms/Web/Statics/Scripts/Registrators'

// Roots
import '../../../WebWorms/Web/Statics/Scripts/Extensions/Browser'
import '../../../WebWorms/Web/Statics/Scripts/Extensions/UnderscoreAddon'
import '../../../WebWorms/Web/Statics/Scripts/Expressions/Expression'
import '../../../WebWorms/Web/Statics/Scripts/AutoLoad/Architecture/Sandbox'
import '../../../WebWorms/Web/Statics/Scripts/AutoLoad/Architecture/SandboxIds'

import '../../../WebWorms/Web/Blocks/Styles'
import '../../../WebWorms/Web/Blocks/AdminTools/Styles'

// Used in inline scripts
import '../../../WebWorms/Web/Statics/Scripts/AutoLoad/Elements/IndependentControls/ObjectTreeValue/ObjectTreeViewerRegistrator'

// DomReady controls
import '../../../WebWorms/Web/Blocks/Unregistered/Unregistered'
import '../../../WebWorms/Web/Blocks/Calendar/Calendar'
import '../../../WebWorms/Web/Blocks/AdminTools/AdminEntities/AdminEntities'
import '../../../WebWorms/Web/Blocks/AdminTools/AdminToolsAlphabetList/AdminToolsAlphabetList'
import '../../../WebWorms/Web/Blocks/AdminTools/AdminToolsUpper/AdminToolsUpper'
import '../../../WebWorms/Web/Statics/KonturModules/KonturCommonDesignLocal/KonturDropdown'
import '../../../WebWorms/Web/Statics/Scripts/AutoLoad/Services/Click'

import {registerReferences} from '../../../WebWorms/Web/Statics/Scripts/References'
registerReferences();

import {registerWebWormsClickActions} from '../../../WebWorms/Web/Statics/Scripts/ClickActions'
registerWebWormsClickActions();
import {registerEvaluators} from '../../../WebWorms/Web/Statics/Scripts/AutoLoad/Elements/Evaluation/RegisterEvaluators'
registerEvaluators();
import {registerValidators} from  '../../../WebWorms/Web/Statics/Scripts/AutoLoad/Elements/Fields/ValidatedField/Validation/ValidationNames'
registerValidators();
import {registerWebWormControllers} from  '../../../WebWorms/Web/Statics/Scripts/Controllers'
registerWebWormControllers();
