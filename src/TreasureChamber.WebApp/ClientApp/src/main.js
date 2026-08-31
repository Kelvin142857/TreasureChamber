import { createApp } from 'vue'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'

import 'primevue/resources/themes/lara-light-blue/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeicons/primeicons.css'
import './assets/app.css'

import App from './App.vue'
import router from './router'

// 全局注册 PrimeVue 组件
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import SelectButton from 'primevue/selectbutton'
import Dropdown from 'primevue/dropdown'
import Checkbox from 'primevue/checkbox'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Paginator from 'primevue/paginator'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import Badge from 'primevue/badge'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import Galleria from 'primevue/galleria'
import FileUpload from 'primevue/fileupload'
import Menubar from 'primevue/menubar'
import Breadcrumb from 'primevue/breadcrumb'
import Message from 'primevue/message'
import ProgressSpinner from 'primevue/progressspinner'
import Toolbar from 'primevue/toolbar'
import Tree from 'primevue/tree'
import Divider from 'primevue/divider'

const app = createApp(App)
app.use(PrimeVue, { ripple: true })
app.use(ToastService)
app.use(ConfirmationService)
app.use(router)

const components = {
  Button, InputText, InputNumber, Textarea, SelectButton, Dropdown, Checkbox,
  DataTable, Column, Paginator, Card, Tag, Badge, Dialog, Toast, ConfirmDialog,
  Galleria, FileUpload, Menubar, Breadcrumb, Message, ProgressSpinner, Toolbar,
  Tree, Divider
}
for (const [name, comp] of Object.entries(components)) app.component(name, comp)

app.mount('#app')
