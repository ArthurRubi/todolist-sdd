import { render, screen } from '@testing-library/react'

describe('infraestrutura de componentes', () => {
  it('renderiza conteúdo no DOM de teste', () => {
    render(<p>Ambiente de teste pronto</p>)

    expect(screen.getByText('Ambiente de teste pronto')).toBeInTheDocument()
  })
})
