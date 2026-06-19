import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import { definePreset } from '@primevue/themes'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Dialog from 'primevue/dialog'
import Password from 'primevue/password'
import Message from 'primevue/message'
import InputNumber from 'primevue/inputnumber'
import SelectButton from 'primevue/selectbutton'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import DatePicker from 'primevue/datepicker'
import Checkbox from 'primevue/checkbox'
import 'primeicons/primeicons.css'
import './assets/styles/main.css'
import App from './App.vue'
import router from './router'

const ParkingTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50:  '{blue.50}',
      100: '{blue.100}',
      200: '{blue.200}',
      300: '{blue.300}',
      400: '{blue.400}',
      500: '{blue.500}',
      600: '{blue.600}',
      700: '{blue.700}',
      800: '{blue.800}',
      900: '#003c90',
      950: '#002c6e',
    },
    colorScheme: {
      light: {
        primary: {
          color: '#003c90',
          contrastColor: '#ffffff',
          hoverColor: '#0f52ba',
          activeColor: '#003080',
        },
        surface: {
          0:   '#ffffff',
          50:  '#faf8ff',
          100: '#e9edff',
          200: '#e2e8fc',
          300: '#d4d9ed',
          400: '#9ca3b0',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#2a303f',
          900: '#1e293b',
          950: '#0f172a',
        }
      }
    }
  }
})

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: ParkingTheme,
    options: {
      prefix: 'p',
      darkModeSelector: '.dark',
      cssLayer: false
    }
  }
})
app.use(ToastService)
app.use(ConfirmationService)

app.component('Button', Button)
app.component('DataTable', DataTable)
app.component('Column', Column)
app.component('Tag', Tag)
app.component('Chart', Chart)
app.component('ProgressSpinner', ProgressSpinner)
app.component('InputText', InputText)
app.component('Select', Select)
app.component('Dialog', Dialog)
app.component('Password', Password)
app.component('Message', Message)
app.component('InputNumber', InputNumber)
app.component('SelectButton', SelectButton)
app.component('IconField', IconField)
app.component('InputIcon', InputIcon)
app.component('DatePicker', DatePicker)
app.component('Checkbox', Checkbox)

app.mount('#app')
